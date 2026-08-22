using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Sell.Components;

public partial class EWalletModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsCashIn), nameof(AmountLabel), nameof(SettlementLabel),
        nameof(SettlementDisplay), nameof(WalletAfterDisplay))]
    private bool cashIn = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(RateHint), nameof(RateHintTone), nameof(FeeLineCaption),
        nameof(SettlementDisplay), nameof(WalletAfterDisplay), nameof(CanConfirm))]
    private string amountText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(RateHint), nameof(RateHintTone), nameof(FeeLineCaption),
        nameof(SettlementDisplay), nameof(CanConfirm))]
    private string feeText = "";

    [ObservableProperty]
    private bool isBusy;

    public bool IsCashIn => CashIn;

    private decimal Amount =>
        decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value : 0m;

    private decimal Fee =>
        decimal.TryParse(FeeText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value : 0m;

    private decimal ExpectedWallet => _shell.Day.Current?.ExpectedEWalletBalance ?? 0m;

    public string AmountLabel =>
        CashIn ? "AMOUNT SENT TO THEIR E-WALLET" : "AMOUNT THEY SENT YOU";

    public string RateHint => Amount > 0m && Fee > 0m
        ? $"{Peso.Format(Fee)} on {Peso.Format(Amount)} · {Fee / Amount * 100m:0.0}%"
        : "";

    public string RateHintTone =>
        Amount > 0m && Fee > 0m && Fee / Amount * 100m < 0.5m ? "Gold" : "Ink3";

    public string FeeLineCaption =>
        $"Rings as a normal sale line — E-wallet fee × {(Fee > 0m ? Fee : 0m):0.##} @ ₱1.00. "
        + "The fee is the only revenue.";

    public string SettlementLabel => CashIn ? "COLLECT FROM CUSTOMER" : "HAND CUSTOMER";

    private decimal Settlement => CashIn ? Amount + Fee : Amount - Fee;

    public string SettlementDisplay => Peso.Format(Settlement);

    public string WalletAfterDisplay =>
        Peso.Format(ExpectedWallet + (CashIn ? -Amount : Amount));

    public bool CanConfirm => Amount > 0m && Fee >= 0m && Fee < Amount;

    public EWalletModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void SetCashIn() => CashIn = true;

    [RelayCommand]
    private void SetCashOut() => CashIn = false;

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy || !CanConfirm)
        {
            return;
        }
        IsBusy = true;
        try
        {
            var amount = Amount;
            var settlement = Settlement;
            await _shell.ShiftApi.RecordEWalletAsync(
                CashIn ? "CashIn" : "CashOut", amount, Fee);
            await _shell.RefreshShiftAsync();
            _shell.CloseModal();
            _shell.ShowToast(CashIn
                ? $"Cash in {Peso.Format(amount)} — collect {Peso.Format(settlement)}"
                : $"Cash out {Peso.Format(amount)} — hand {Peso.Format(settlement)}");
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
}
