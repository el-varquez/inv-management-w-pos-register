using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Sell.Components;

public partial class QtyModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => ConfirmCommand;
    public System.Windows.Input.ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public CartLine Line { get; }

    public QtyModalViewModel(AppShell.ShellViewModel shell, CartLine line)
    {
        _shell = shell;
        Line = line;
    }

    [RelayCommand]
    private void Confirm() => _shell.CloseModal();

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
