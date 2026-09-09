using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Store;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class DiscountModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly CartStore _cart;

    [ObservableProperty]
    private string amountText = "";

    public DiscountModalViewModel(AppShell.ShellViewModel shell, CartStore cart)
    {
        _shell = shell;
        _cart = cart;
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

    [RelayCommand]
    private void Confirm()
    {
        _cart.RequestedDiscount =
            decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;
        _shell.CloseModal();
        _shell.FocusScan();
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
