using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using AppShell = POS.Register.Shell;
using UtangScreens = POS.Register.Features.Utang.Screens;

namespace POS.Register.Features.Utang.Components;

public partial class EditPaymentModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => SaveCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly UtangScreens.UtangScreenViewModel _screen;
    private readonly UtangScreens.LedgerRow _row;
    private readonly string _adminToken;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string amountText = "";

    [ObservableProperty]
    private bool isBusy;

    public string Caption =>
        $"Current amount: {Peso.Format(_row.Amount)} — the original stays visible on the ledger.";

    private decimal Amount =>
        decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value : 0m;

    public EditPaymentModalViewModel(
        AppShell.ShellViewModel shell,
        UtangScreens.UtangScreenViewModel screen,
        UtangScreens.LedgerRow row,
        string adminToken)
    {
        _shell = shell;
        _screen = screen;
        _row = row;
        _adminToken = adminToken;
    }

    partial void OnAmountTextChanged(string value)
    {
        var cleaned = new string(value.Where(c => char.IsAsciiDigit(c) || c == '.').ToArray());
        if (cleaned != value)
        {
            AmountText = cleaned;
        }
    }

    [RelayCommand]
    private void Digit(string key) => AmountText += key;

    [RelayCommand]
    private void Backspace() => AmountText = AmountText.Length > 0 ? AmountText[..^1] : "";

    private bool CanSave() => !IsBusy && Amount > 0m;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }
        IsBusy = true;
        SaveCommand.NotifyCanExecuteChanged();
        try
        {
            var amount = Amount;
            await _shell.UtangApi.EditPaymentAsync(_row.Id, amount, _adminToken);
            _shell.CloseModal();
            _shell.ShowToast($"Payment updated to {Peso.Format(amount)}");
            await _screen.RefreshAfterActionAsync();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
