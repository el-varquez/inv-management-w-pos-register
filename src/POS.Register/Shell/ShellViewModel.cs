using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Features.Auth.Screens;
using POS.Register.Features.Sales.Screens;
using POS.Register.Features.Sell.Screens;
using POS.Register.Features.Shifts.Screens;
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

    public DayState Day { get; } = new();
    public LoginScreenViewModel Login { get; }
    public SellScreenViewModel Sell { get; }
    public SalesScreenViewModel Sales { get; }
    public ShiftScreenViewModel Shift { get; }

    public SessionStore Session => _services.Session;
    public SettingsStore Settings => _services.Settings;
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
        Shift = new ShiftScreenViewModel(this);
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
            "Shift" => Shift,
            _ => Sell,
        };
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
        if (ReferenceEquals(CurrentScreen, Sell) && Sell.ShowResults)
        {
            Sell.ClearSearch();
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

    public void ShowToast(string message)
    {
        Toast = message;
        _toastTimer.Stop();
        _toastTimer.Start();
    }
}
