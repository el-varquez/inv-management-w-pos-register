using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class AdminOverrideModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => ConfirmCommand;
    public System.Windows.Input.ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly Action _onApproved;

    public string Reason { get; }

    public AdminOverrideModalViewModel(AppShell.ShellViewModel shell, string reason, Action onApproved)
    {
        _shell = shell;
        _onApproved = onApproved;
        Reason = reason;
    }

    [RelayCommand]
    private void Confirm()
    {
        _shell.CloseModal();
        _onApproved();
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
