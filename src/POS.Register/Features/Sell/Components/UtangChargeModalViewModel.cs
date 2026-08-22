using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Sell.Components;

public partial class SukiRow : ObservableObject
{
    public SukiDto Suki { get; }
    public string Initial => Suki.Name.Length > 0 ? Suki.Name[..1] : "?";
    public string Name => Suki.Name;
    public string PhoneDisplay => string.IsNullOrWhiteSpace(Suki.Phone) ? "—" : Suki.Phone;
    public string BalanceDisplay => Peso.Format(Suki.Balance);
    public bool IsZeroBalance => Suki.Balance == 0m;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isHighlighted;

    public SukiRow(SukiDto suki, bool selected)
    {
        Suki = suki;
        isSelected = selected;
    }
}

public partial class UtangChargeModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => DefaultActionCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly DispatcherTimer _searchTimer;
    private bool _suppressSearch;

    public ObservableCollection<SukiRow> Rows { get; } = [];
    public MoneyEntry Down { get; } = new();

    [ObservableProperty]
    private string search = "";

    [ObservableProperty]
    private int resultIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommitLabel))]
    [NotifyCanExecuteChangedFor(nameof(CommitCommand))]
    private SukiDto? selectedSuki;

    [ObservableProperty]
    private bool addingSuki;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveSukiCommand))]
    private string newName = "";

    [ObservableProperty]
    private string newPhone = "";

    [ObservableProperty]
    private bool isBusy;

    public decimal UtangTotal =>
        _shell.Cart.Lines.Sum(l =>
            (l.Price + (l.UtangMarkup ?? _shell.Settings.DefaultUtangMarkup)) * l.Qty)
        - _shell.Cart.Discount;

    public string TotalDisplay => Peso.Format(UtangTotal);

    private decimal DownValue => Math.Min(Down.Value, UtangTotal);

    public decimal ChargeAmount => UtangTotal - DownValue;

    public string CommitLabel => SelectedSuki is { } suki
        ? $"CHARGE {Peso.Format(ChargeAmount)} TO {suki.Name.ToUpperInvariant()} — NEW BAL {Peso.Format(suki.Balance + ChargeAmount)}"
        : "SELECT A CUSTOMER";

    public UtangChargeModalViewModel(AppShell.ShellViewModel shell)
    {
        _shell = shell;
        Down.PropertyChanged += (_, _) => OnPropertyChanged(nameof(CommitLabel));
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _ = RunSearchAsync(Search.Trim());
        };
    }

    partial void OnSearchChanged(string value)
    {
        _searchTimer.Stop();
        if (_suppressSearch)
        {
            return;
        }
        if (value.Trim().Length == 0)
        {
            Rows.Clear();
            ResultIndex = -1;
            return;
        }
        _searchTimer.Start();
    }

    partial void OnResultIndexChanged(int value)
    {
        for (var i = 0; i < Rows.Count; i++)
        {
            Rows[i].IsHighlighted = i == value;
        }
    }

    [RelayCommand]
    private void SelectNext()
    {
        if (Rows.Count == 0)
        {
            return;
        }
        ResultIndex = Math.Min(Rows.Count - 1, ResultIndex + 1);
    }

    [RelayCommand]
    private void SelectPrev() => ResultIndex = Math.Max(-1, ResultIndex - 1);

    [RelayCommand]
    private void DefaultAction()
    {
        if (ResultIndex >= 0 && ResultIndex < Rows.Count)
        {
            SelectAndCollapse(Rows[ResultIndex].Suki);
            return;
        }
        if (CommitCommand.CanExecute(null))
        {
            CommitCommand.Execute(null);
        }
    }

    partial void OnSelectedSukiChanged(SukiDto? value)
    {
        foreach (var row in Rows)
        {
            row.IsSelected = row.Suki.Id == value?.Id;
        }
    }

    private void SelectAndCollapse(SukiDto suki)
    {
        SelectedSuki = suki;
        _suppressSearch = true;
        Search = suki.Name;
        _suppressSearch = false;
        Rows.Clear();
        ResultIndex = -1;
    }

    private async Task RunSearchAsync(string term)
    {
        if (term.Length == 0 || term != Search.Trim())
        {
            return;
        }
        try
        {
            var sukis = await _shell.UtangApi.GetSukisAsync(term);
            if (term != Search.Trim())
            {
                return;
            }
            Rows.Clear();
            foreach (var suki in sukis)
            {
                Rows.Add(new SukiRow(suki, suki.Id == SelectedSuki?.Id));
            }
            ResultIndex = -1;
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void PickSuki(SukiRow row) => SelectAndCollapse(row.Suki);

    [RelayCommand]
    private void ToggleAddSuki()
    {
        AddingSuki = !AddingSuki;
        NewName = "";
        NewPhone = "";
    }

    private bool CanSaveSuki() => NewName.Trim().Length > 0;

    [RelayCommand(CanExecute = nameof(CanSaveSuki))]
    private async Task SaveSukiAsync()
    {
        try
        {
            var created = await _shell.UtangApi.CreateSukiAsync(
                NewName.Trim(),
                string.IsNullOrWhiteSpace(NewPhone) ? null : NewPhone.Trim());
            AddingSuki = false;
            NewName = "";
            NewPhone = "";
            SelectAndCollapse(created);
            _ = _shell.RefreshUtangAsync();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void Digit(string key)
    {
        Down.Push(key);
        OnPropertyChanged(nameof(CommitLabel));
    }

    [RelayCommand]
    private void Backspace()
    {
        Down.Backspace();
        OnPropertyChanged(nameof(CommitLabel));
    }

    private bool CanCommit() => SelectedSuki is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanCommit))]
    private async Task CommitAsync()
    {
        if (IsBusy || SelectedSuki is not { } suki)
        {
            return;
        }
        IsBusy = true;
        CommitCommand.NotifyCanExecuteChanged();
        try
        {
            var items = _shell.Cart.Lines
                .Select(l => new SaleItemInput(l.ItemId, l.Qty, 0m))
                .ToList();
            var down = DownValue;

            var result = await _shell.SellApi.CompleteSaleAsync(
                items, _shell.Cart.Discount, "Utang", 0m, null, suki.Id, down);

            _shell.Cart.Clear();
            _ = _shell.RefreshShiftAsync();
            _ = _shell.RefreshUtangAsync();
            _shell.CloseModal();
            _shell.ShowToast(
                $"Charged {Peso.Format(result.Total - down)} to {suki.Name} · {result.ReceiptNumber}");
            _shell.FocusScan();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
            CommitCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
