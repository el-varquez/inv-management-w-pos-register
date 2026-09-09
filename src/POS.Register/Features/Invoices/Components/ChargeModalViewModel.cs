using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Invoices.Components;

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

public partial class ChargeModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => DefaultActionCommand;
    public ICommand DismissCommand => DismissStepCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly DispatcherTimer _searchTimer;
    private bool _suppressSearch;
    private bool _overdueConfirmed;

    public ObservableCollection<SukiRow> Rows { get; } = [];

    [ObservableProperty]
    private string search = "";

    [ObservableProperty]
    private int resultIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommitLabel), nameof(ShowOverdue), nameof(OverdueLine), nameof(OverdueQuestion))]
    [NotifyCanExecuteChangedFor(nameof(ChargeCommand))]
    private SukiDto? selectedSuki;

    [ObservableProperty]
    private bool addingSuki;

    [ObservableProperty]
    private bool awaitingOverdueConfirm;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveSukiCommand))]
    private string newName = "";

    [ObservableProperty]
    private string newPhone = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChargeCommand))]
    private bool isBusy;

    public decimal Subtotal => _shell.InvoiceCart.Subtotal;
    public decimal MarkupTotal => _shell.InvoiceCart.Lines
        .Sum(l => (l.UtangMarkup ?? _shell.Settings.DefaultUtangMarkup) * l.Qty);
    public decimal Discount => _shell.InvoiceCart.Discount;
    public decimal Total => Subtotal + MarkupTotal - Discount;

    public string SubtotalDisplay => Peso.Format(Subtotal);
    public string MarkupDisplay => "+" + Peso.Format(MarkupTotal);
    public string DiscountDisplay => Discount > 0 ? "−" + Peso.Format(Discount) : Peso.Format(0m);
    public string TotalDisplay => Peso.Format(Total);

    public bool ShowOverdue => SelectedSuki?.PaymentOverdue == true;

    public string OverdueLine => SelectedSuki is { } suki
        ? $"No payment in {suki.DaysSincePayment} days · owes {Peso.Format(suki.Balance)}"
        : "";

    public string OverdueQuestion => SelectedSuki is { } suki
        ? $"{suki.Name} hasn't paid in {suki.DaysSincePayment} days — charge anyway?"
        : "";

    public string CommitLabel => SelectedSuki is { } suki
        ? $"CHARGE {TotalDisplay} TO {suki.Name.ToUpperInvariant()} — NEW BAL {Peso.Format(suki.Balance + Total)}"
        : "SELECT A CUSTOMER";

    public ChargeModalViewModel(AppShell.ShellViewModel shell)
    {
        _shell = shell;
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

    partial void OnSelectedSukiChanged(SukiDto? value)
    {
        _overdueConfirmed = false;
        AwaitingOverdueConfirm = false;
        foreach (var row in Rows)
        {
            row.IsSelected = row.Suki.Id == value?.Id;
        }
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

    private void SelectAndCollapse(SukiDto suki)
    {
        SelectedSuki = suki;
        _suppressSearch = true;
        Search = suki.Name;
        _suppressSearch = false;
        Rows.Clear();
        ResultIndex = -1;
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
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void DefaultAction()
    {
        if (AwaitingOverdueConfirm)
        {
            ConfirmOverdueCommand.Execute(null);
            return;
        }
        if (ResultIndex >= 0 && ResultIndex < Rows.Count)
        {
            SelectAndCollapse(Rows[ResultIndex].Suki);
            return;
        }
        if (ChargeCommand.CanExecute(null))
        {
            ChargeCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void ConfirmOverdue()
    {
        _overdueConfirmed = true;
        AwaitingOverdueConfirm = false;
        if (ChargeCommand.CanExecute(null))
        {
            ChargeCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void CancelOverdue() => AwaitingOverdueConfirm = false;

    private bool CanCharge() => !IsBusy && !_shell.InvoiceCart.IsEmpty && SelectedSuki is not null;

    [RelayCommand(CanExecute = nameof(CanCharge))]
    private async Task ChargeAsync()
    {
        if (SelectedSuki is not { } suki)
        {
            return;
        }
        if (ShowOverdue && !_overdueConfirmed)
        {
            AwaitingOverdueConfirm = true;
            return;
        }
        IsBusy = true;
        try
        {
            var items = _shell.InvoiceCart.Lines
                .Select(l => new SaleItemInput(l.ItemId, l.Qty, 0m))
                .ToList();
            var result = await _shell.InvoiceApi.CreateAsync(items, _shell.InvoiceCart.Discount, suki.Id);
            _shell.InvoiceCart.Clear();
            _ = _shell.RefreshShiftAsync();
            _shell.CloseModal();
            _shell.ShowToast(
                $"Charged {Peso.Format(result.Total)} to {suki.Name} · {result.InvoiceNumber} — new balance {Peso.Format(result.NewBalance)}");
            _shell.FocusScan();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();

    [RelayCommand]
    private void DismissStep()
    {
        if (AwaitingOverdueConfirm)
        {
            AwaitingOverdueConfirm = false;
            return;
        }
        Cancel();
    }
}
