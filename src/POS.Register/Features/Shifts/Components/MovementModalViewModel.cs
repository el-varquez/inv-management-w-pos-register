using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Shifts.Components;

public partial class MovementModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => ConfirmCommand;
    public System.Windows.Input.ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpectedAfterDisplay))]
    private bool isPayout = true;

    public string AmountDisplay => CannedDay.MovementAmount;
    public string NoteDisplay => CannedDay.MovementNote;
    public string ExpectedAfterDisplay => IsPayout ? CannedDay.PayoutExpectedAfter : CannedDay.PayInExpectedAfter;

    public MovementModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void SetPayout() => IsPayout = true;

    [RelayCommand]
    private void SetPayIn() => IsPayout = false;

    [RelayCommand]
    private void Confirm()
    {
        _shell.CloseModal();
        _shell.ShowToast(IsPayout ? "Payout recorded" : "Pay-in recorded");
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
