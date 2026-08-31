using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Components;

public record QuickBill(string Label, decimal Amount);

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

public partial class PaymentModalViewModel : ObservableObject, POS.Register.Shell.IDefaultAction
{
    public ICommand DefaultCommand => DefaultActionCommand;
    public ICommand DismissCommand => DismissStepCommand;

    private readonly POS.Register.Shell.ShellViewModel _shell;
    private readonly DispatcherTimer _searchTimer;
    private bool _suppressSearch;

    public IReadOnlyList<PaymentMethodDto> Methods => _shell.Methods.ActiveMethods;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsCash), nameof(IsInvoice), nameof(ShowReference), nameof(SelectedName),
        nameof(EwalletCaption), nameof(ReferencePlaceholder), nameof(Title),
        nameof(AmountCaption), nameof(AmountDueDisplay), nameof(CommitLabel))]
    [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
    private PaymentMethodDto? selectedMethod;

    [ObservableProperty]
    private bool isMethodOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChangeDisplay), nameof(IsInsufficient))]
    [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
    private string tenderedText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
    private string referenceText = "";

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<SukiRow> Rows { get; } = [];
    public MoneyEntry Down { get; } = new();

    [ObservableProperty]
    private string search = "";

    [ObservableProperty]
    private int resultIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommitLabel))]
    [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
    private SukiDto? selectedSuki;

    [ObservableProperty]
    private bool addingSuki;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveSukiCommand))]
    private string newName = "";

    [ObservableProperty]
    private string newPhone = "";

    public bool IsCash => SelectedMethod?.Id == KnownPaymentMethods.Cash;
    public bool IsInvoice => SelectedMethod?.Type == "Invoice";
    public bool ShowReference => !IsCash && !IsInvoice;
    public string SelectedName => SelectedMethod?.Name.ToUpperInvariant() ?? "";
    public string Title => IsInvoice ? $"{SelectedMethod?.Name} — charge to customer" : "Payment";
    public decimal Total => _shell.Cart.Total;
    public string TotalDisplay => Peso.Format(Total);
    public string AmountCaption => IsInvoice ? "SALE TOTAL" : "AMOUNT DUE";
    public string AmountDueDisplay => IsInvoice ? Peso.Format(UtangTotal) : TotalDisplay;
    public string EwalletCaption
        => $"Customer pays {TotalDisplay} via {SelectedMethod?.Name} — enter the reference number to complete.";
    public string ReferencePlaceholder => $"{SelectedMethod?.Name} reference number";
    public IReadOnlyList<QuickBill> QuickBills { get; }

    private decimal Tendered =>
        decimal.TryParse(TenderedText, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    public bool IsInsufficient => Tendered < Total;

    public string ChangeDisplay => IsInsufficient
        ? $"Insufficient — need {Peso.Format(Total - Tendered)}"
        : Peso.Format(Tendered - Total);

    public decimal UtangTotal =>
        _shell.Cart.Lines.Sum(l =>
            (l.Price + (l.UtangMarkup ?? _shell.Settings.DefaultUtangMarkup)) * l.Qty)
        - _shell.Cart.Discount;

    private decimal DownValue => Math.Min(Down.Value, UtangTotal);

    public decimal ChargeAmount => UtangTotal - DownValue;

    public string CommitLabel => SelectedSuki is { } suki
        ? $"CHARGE {Peso.Format(ChargeAmount)} TO {suki.Name.ToUpperInvariant()} — NEW BAL {Peso.Format(suki.Balance + ChargeAmount)}"
        : "SELECT A CUSTOMER";

    public PaymentModalViewModel(POS.Register.Shell.ShellViewModel shell)
    {
        _shell = shell;
        selectedMethod = Methods.FirstOrDefault(m => m.Id == KnownPaymentMethods.Cash)
            ?? Methods.FirstOrDefault();
        tenderedText = Total.ToString("0.##", CultureInfo.InvariantCulture);
        var bills = new List<QuickBill> { new("EXACT", Total) };
        var seen = new HashSet<decimal>();
        foreach (var denom in new[] { 20m, 50m, 100m, 200m, 500m, 1000m })
        {
            var value = Math.Ceiling(Total / denom) * denom;
            if (value > 0 && value != Total && seen.Add(value))
            {
                bills.Add(new QuickBill("₱" + value.ToString("N0", CultureInfo.InvariantCulture), value));
            }
        }
        QuickBills = bills;
        Down.PropertyChanged += (_, _) => OnPropertyChanged(nameof(CommitLabel));
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _ = RunSearchAsync(Search.Trim());
        };
    }

    partial void OnTenderedTextChanged(string value)
    {
        var cleaned = new string(value.Where(c => char.IsAsciiDigit(c) || c == '.').ToArray());
        if (cleaned != value)
        {
            TenderedText = cleaned;
        }
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
        foreach (var row in Rows)
        {
            row.IsSelected = row.Suki.Id == value?.Id;
        }
    }

    [RelayCommand]
    private void Digit(string key)
    {
        if (IsInvoice)
        {
            Down.Push(key);
            OnPropertyChanged(nameof(CommitLabel));
            return;
        }
        TenderedText += key;
    }

    [RelayCommand]
    private void Backspace()
    {
        if (IsInvoice)
        {
            Down.Backspace();
            OnPropertyChanged(nameof(CommitLabel));
            return;
        }
        TenderedText = TenderedText.Length > 0 ? TenderedText[..^1] : "";
    }

    [RelayCommand]
    private void PickBill(QuickBill bill)
        => TenderedText = bill.Amount.ToString("0.##", CultureInfo.InvariantCulture);

    [RelayCommand]
    private void ToggleMethod() => IsMethodOpen = !IsMethodOpen;

    [RelayCommand]
    private void PickMethod(PaymentMethodDto method)
    {
        SelectedMethod = method;
        IsMethodOpen = false;
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
        if (IsInvoice && ResultIndex >= 0 && ResultIndex < Rows.Count)
        {
            SelectAndCollapse(Rows[ResultIndex].Suki);
            return;
        }
        if (CompleteCommand.CanExecute(null))
        {
            CompleteCommand.Execute(null);
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

    private bool CanComplete()
    {
        if (IsBusy || _shell.Cart.IsEmpty || SelectedMethod is null)
        {
            return false;
        }
        if (IsCash)
        {
            return !IsInsufficient;
        }
        if (IsInvoice)
        {
            return SelectedSuki is not null;
        }
        return !SelectedMethod.RequiresReference || ReferenceText.Trim().Length > 0;
    }

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private async Task CompleteAsync()
    {
        if (IsBusy || SelectedMethod is not { } method)
        {
            return;
        }
        IsBusy = true;
        CompleteCommand.NotifyCanExecuteChanged();
        try
        {
            var items = _shell.Cart.Lines
                .Select(l => new SaleItemInput(l.ItemId, l.Qty, 0m))
                .ToList();

            if (IsInvoice && SelectedSuki is { } suki)
            {
                var down = DownValue;
                var result = await _shell.SellApi.CompleteSaleAsync(
                    items, _shell.Cart.Discount, method.Id, 0m, null, suki.Id, down);
                _shell.Cart.Clear();
                _ = _shell.RefreshShiftAsync();
                _ = _shell.RefreshUtangAsync();
                _shell.CloseModal();
                _shell.ShowToast(
                    $"Charged {Peso.Format(result.Total - down)} to {suki.Name} · {result.ReceiptNumber}");
                _shell.FocusScan();
            }
            else if (IsCash)
            {
                var result = await _shell.SellApi.CompleteSaleAsync(
                    items, _shell.Cart.Discount, method.Id, Tendered, null);
                _shell.Cart.Clear();
                _ = _shell.RefreshShiftAsync();
                _shell.CloseModal();
                _shell.OpenModal(new SuccessModalViewModel(
                    _shell, $"Receipt {result.ReceiptNumber} · {method.Name}", Peso.Format(result.Change)));
            }
            else
            {
                var reference = ReferenceText.Trim();
                var result = await _shell.SellApi.CompleteSaleAsync(
                    items, _shell.Cart.Discount, method.Id, Total,
                    reference.Length > 0 ? reference : null);
                _shell.Cart.Clear();
                _ = _shell.RefreshShiftAsync();
                _shell.CloseModal();
                _shell.ShowToast($"{method.Name} sale completed · {result.ReceiptNumber}");
                _shell.FocusScan();
            }
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
            CompleteCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();

    [RelayCommand]
    private void DismissStep()
    {
        if (IsMethodOpen)
        {
            IsMethodOpen = false;
            return;
        }
        Cancel();
    }
}
