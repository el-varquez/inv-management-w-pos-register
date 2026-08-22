using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Features.Auth.Screens;
using POS.Register.Features.Sales.Screens;
using POS.Register.Features.Sell.Screens;
using POS.Register.Features.Shifts.Screens;
using POS.Register.Features.Utang.Screens;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Store;
using POS.Register.Types;

namespace POS.Register.Shell;

public partial class ShellViewModel : ObservableObject
{
    private readonly DispatcherTimer _toastTimer;
    private readonly DispatcherTimer _clockTimer;
    private readonly AppServices _services;

    public LoginScreenViewModel Login { get; }
    public SellScreenViewModel Sell { get; }
    public SalesScreenViewModel Sales { get; }
    public UtangScreenViewModel UtangScreen { get; }
    public ShiftScreenViewModel Shift { get; }

    public SessionStore Session => _services.Session;
    public SettingsStore Settings => _services.Settings;
    public ShiftStore Day => _services.ShiftState;
    public CartStore Cart => _services.Cart;
    public UtangStore Utang => _services.UtangState;
    public UtangService UtangApi => _services.Utang;
    public bool ShowUtangRail => Settings.AcceptUtang || Utang.HasAnyBalance;
    public ShiftService ShiftApi => _services.Shifts;
    public DayService DayApi => _services.Days;
    public POS.Register.Features.Sell.Services.SellService SellApi => _services.Sell;
    public POS.Register.Features.Sales.Services.SalesService SalesApi => _services.Sales;

    public Task<LoginResponse> AuthenticateAsync(string username, string password)
        => _services.Auth.LoginAsync(username, password);
    public string DateShort => DateTime.Now.ToString("ddd, MMM d");

    [ObservableProperty]
    private bool isLoggedIn;

    [ObservableProperty]
    private object? currentScreen;

    [ObservableProperty]
    private string activeScreenName = "Sell";

    [ObservableProperty]
    private object? activeModal;

    [ObservableProperty]
    private string? toast;

    [ObservableProperty]
    private string clock = DateTime.Now.ToString("h:mm tt");

    public ShellViewModel()
    {
        _services = new AppServices();
        _services.SessionExpired += OnSessionExpired;
        Login = new LoginScreenViewModel(this, _services.Auth, _services.Settings);
        Sell = new SellScreenViewModel(this);
        Sales = new SalesScreenViewModel(this);
        UtangScreen = new UtangScreenViewModel(this);
        Shift = new ShiftScreenViewModel(this);
        Settings.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ShowUtangRail));
        Utang.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ShowUtangRail));
        CurrentScreen = Sell;
        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.4) };
        _toastTimer.Tick += (_, _) => { Toast = null; _toastTimer.Stop(); };
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => Clock = DateTime.Now.ToString("h:mm tt");
        _clockTimer.Start();
        _ = LoadStoreNameAsync();
    }

    public void EnterSession(LoginResponse result)
    {
        Session.Set(result.Token!, result.Name ?? "", result.Username ?? "", result.Role ?? "");
        IsLoggedIn = true;
        Navigate("Sell");
        _ = LoadStoreNameAsync();
        _ = LoadSettingsAsync();
        _ = RefreshShiftAsync();
        _ = RefreshUtangAsync();
        _ = Sell.RefreshPopularAsync();
    }

    public async Task RefreshUtangAsync()
    {
        try
        {
            Utang.Set(await _services.Utang.GetSukisAsync());
        }
        catch (ApiException)
        {
        }
    }

    public async Task RefreshShiftAsync()
    {
        try
        {
            Day.CurrentDay = await _services.Days.GetCurrentAsync();
            Day.Current = await _services.Shifts.GetCurrentAsync();
            if (Day.Current is null)
            {
                var page = await _services.Shifts.GetShiftsAsync(1, 1);
                Day.LatestClosed = page.Items.Count > 0
                    ? await _services.Shifts.GetReadAsync(page.Items[0].Id)
                    : null;
            }
            if (Day.CurrentDay is null)
            {
                var days = await _services.Days.GetDaysAsync(1, 1);
                Day.LatestClosedDay = days.Items.Count > 0
                    ? await _services.Days.GetReadAsync(days.Items[0].Id)
                    : null;
            }
            else
            {
                Day.LatestClosedDay = null;
            }
        }
        catch (ApiException ex)
        {
            ShowToast(ex.Message);
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _services.StoreSettings.GetSettingsAsync();
            Settings.StoreName = settings.StoreName;
            Settings.AcceptUtang = settings.AcceptUtang;
            Settings.DefaultUtangMarkup = settings.DefaultUtangMarkup;
            Settings.TrackEWalletFloat = settings.TrackEWalletFloat;
        }
        catch (ApiException)
        {
        }
    }

    private async Task LoadStoreNameAsync()
    {
        try
        {
            Settings.StoreName = (await _services.StoreSettings.GetStoreNameAsync()).StoreName;
        }
        catch (ApiException ex)
        {
            ShowToast(ex.Message);
        }
    }

    private void OnSessionExpired()
    {
        if (!IsLoggedIn)
        {
            return;
        }
        Lock();
        ShowToast("Session expired — log in again.");
    }

    [RelayCommand]
    private void Navigate(string name)
    {
        ActiveScreenName = name;
        CurrentScreen = name switch
        {
            "Sales" => Sales,
            "Utang" => UtangScreen,
            "Shift" => Shift,
            _ => Sell,
        };
        if (name == "Utang" && IsLoggedIn)
        {
            _ = UtangScreen.LoadAsync();
        }
        if (name == "Shift" && IsLoggedIn)
        {
            _ = RefreshShiftAsync();
        }
        if (name == "Sales" && IsLoggedIn)
        {
            _ = Sales.LoadAsync();
        }
    }

    [RelayCommand]
    private void Lock()
    {
        Session.Clear();
        Login.Reset();
        IsLoggedIn = false;
        ActiveModal = null;
    }

    [RelayCommand]
    private void Default()
    {
        if (ActiveModal is IDefaultAction modal)
        {
            if (modal.DefaultCommand.CanExecute(null))
            {
                modal.DefaultCommand.Execute(null);
            }
            return;
        }
        if (!IsLoggedIn)
        {
            Login.Submit();
            return;
        }
        if (ReferenceEquals(CurrentScreen, Sell) && Sell.HasQuery)
        {
            _ = Sell.CommitSearchAsync();
        }
    }

    [RelayCommand]
    private void Dismiss()
    {
        if (ActiveModal is IDefaultAction modal && modal.DismissCommand.CanExecute(null))
        {
            modal.DismissCommand.Execute(null);
        }
    }

    public void OpenModal(object modal) => ActiveModal = modal;

    public void CloseModal() => ActiveModal = null;

    public void FocusScan() => Sell.RequestScanFocus();

    public void ShowToast(string message)
    {
        Toast = message;
        _toastTimer.Stop();
        _toastTimer.Start();
    }
}
