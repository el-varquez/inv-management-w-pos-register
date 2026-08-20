using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class SuccessModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => NewSaleCommand;
    public System.Windows.Input.ICommand DismissCommand => NewSaleCommand;

    private readonly AppShell.ShellViewModel _shell;

    public string ReceiptCaption { get; }
    public string ChangeDisplay { get; }

    public SuccessModalViewModel(AppShell.ShellViewModel shell, string receiptCaption, string changeDisplay)
    {
        _shell = shell;
        ReceiptCaption = receiptCaption;
        ChangeDisplay = changeDisplay;
    }

    [RelayCommand]
    private void NewSale()
    {
        _shell.CloseModal();
        _shell.FocusScan();
    }
}
