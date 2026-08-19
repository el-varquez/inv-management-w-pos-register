using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class StartingCashModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => ConfirmCommand;
    public System.Windows.Input.ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public string AmountDisplay => "2,000";

    public StartingCashModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void Confirm()
    {
        _shell.Day.IsShiftOpen = true;
        _shell.Day.IsClosed = false;
        _shell.CloseModal();
        _shell.ShowToast("Shift #12 opened with ₱2,000.00 starting cash");
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
