using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Components;

public record QuickBill(string Label, decimal Amount);

public partial class PaymentModalViewModel : ObservableObject, POS.Register.Shell.IDefaultAction
{
    public ICommand DefaultCommand => CompleteCommand;
    public ICommand DismissCommand => DismissStepCommand;

    private readonly POS.Register.Shell.ShellViewModel _shell;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCash), nameof(EwalletCaption), nameof(ReferencePlaceholder))]
    [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
    private string method = "CASH";

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

    public bool IsCash => Method == "CASH";
    public decimal Total => _shell.Cart.Total;
    public string TotalDisplay => Peso.Format(Total);
    public string EwalletCaption
        => $"Customer pays {TotalDisplay} via {Method} — enter the reference number to complete.";
    public string ReferencePlaceholder => $"{Method} reference number";
    public IReadOnlyList<QuickBill> QuickBills { get; }

    private decimal Tendered =>
        decimal.TryParse(TenderedText, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    public bool IsInsufficient => Tendered < Total;

    public string ChangeDisplay => IsInsufficient
        ? $"Insufficient — need {Peso.Format(Total - Tendered)}"
        : Peso.Format(Tendered - Total);

    public PaymentModalViewModel(POS.Register.Shell.ShellViewModel shell)
    {
        _shell = shell;
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
    }

    partial void OnTenderedTextChanged(string value)
    {
        var cleaned = new string(value.Where(c => char.IsAsciiDigit(c) || c == '.').ToArray());
        if (cleaned != value)
        {
            TenderedText = cleaned;
        }
    }

    [RelayCommand]
    private void Digit(string key) => TenderedText += key;

    [RelayCommand]
    private void Backspace() => TenderedText = TenderedText.Length > 0 ? TenderedText[..^1] : "";

    [RelayCommand]
    private void PickBill(QuickBill bill)
        => TenderedText = bill.Amount.ToString("0.##", CultureInfo.InvariantCulture);

    [RelayCommand]
    private void ToggleMethod() => IsMethodOpen = !IsMethodOpen;

    [RelayCommand]
    private void PickMethod(string value)
    {
        Method = value;
        IsMethodOpen = false;
    }

    private bool CanComplete()
        => !IsBusy && !_shell.Cart.IsEmpty && (IsCash ? !IsInsufficient : ReferenceText.Trim().Length > 0);

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private async Task CompleteAsync()
    {
        if (IsBusy)
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
            var paymentType = Method switch { "GCASH" => "Gcash", "MAYA" => "Maya", _ => "Cash" };
            var tendered = IsCash ? Tendered : Total;
            var reference = IsCash ? null : ReferenceText.Trim();

            var result = await _shell.SellApi.CompleteSaleAsync(
                items, _shell.Cart.Discount, paymentType, tendered, reference);

            _shell.Cart.Clear();
            _ = _shell.RefreshShiftAsync();
            _shell.CloseModal();
            if (IsCash)
            {
                _shell.OpenModal(new SuccessModalViewModel(
                    _shell, $"Receipt {result.ReceiptNumber} · Cash", Peso.Format(result.Change)));
            }
            else
            {
                _shell.ShowToast($"{Method} sale completed · {result.ReceiptNumber}");
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
