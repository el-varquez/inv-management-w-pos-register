using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class SuccessModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => NewSaleCommand;
    public System.Windows.Input.ICommand DismissCommand => NewSaleCommand;

    private readonly AppShell.ShellViewModel _shell;

    public string ReceiptCaption => CannedDay.SuccessReceipt;
    public string ChangeDisplay => CannedDay.ChangeDisplay;

    public SuccessModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void NewSale() => _shell.CloseModal();
}
