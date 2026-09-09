using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Store;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class QtyModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly CartStore _cart;

    public CartLine Line { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string qtyText;

    public QtyModalViewModel(AppShell.ShellViewModel shell, CartStore cart, CartLine line)
    {
        _shell = shell;
        _cart = cart;
        Line = line;
        qtyText = line.Qty.ToString();
    }

    partial void OnQtyTextChanged(string value)
    {
        var digits = new string(value.Where(char.IsAsciiDigit).ToArray());
        if (digits != value)
        {
            QtyText = digits;
        }
    }

    private int ParsedQty => int.TryParse(QtyText, out var n) ? n : 0;

    private bool CanConfirm() => ParsedQty > 0;

    [RelayCommand]
    private void Digit(string key) => QtyText = (QtyText + key).TrimStart('0');

    [RelayCommand]
    private void Backspace() => QtyText = QtyText.Length > 0 ? QtyText[..^1] : "";

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        if (!_cart.TrySetQty(Line, ParsedQty))
        {
            _shell.ShowToast($"Only {Line.Cap} available");
            return;
        }
        _shell.CloseModal();
        _shell.FocusScan();
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
