using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Sell.Components;

public partial class DiscountModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => ConfirmCommand;
    public System.Windows.Input.ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public DiscountModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void Confirm()
    {
        _shell.CloseModal();
        _shell.ShowToast("Discount applied");
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
