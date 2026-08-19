using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Shifts.Components;

public partial class ZReadModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => CloseShiftCommand;
    public System.Windows.Input.ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public IReadOnlyList<DenominationCount> Denominations => CannedDay.Denominations;
    public string CountedDisplay => Peso.Format(CannedDay.CountedCash);
    public string ExpectedDisplay => Peso.Format(CannedDay.ExpectedCash);
    public string Verdict => CannedDay.ZVerdict;

    public ZReadModalViewModel(AppShell.ShellViewModel shell) => _shell = shell;

    [RelayCommand]
    private void CloseShift()
    {
        _shell.Day.IsShiftOpen = false;
        _shell.Day.IsClosed = true;
        _shell.Day.ClosedLate = true;
        _shell.CloseModal();
        _shell.ShowToast("Shift #12 closed — Z read #12 saved");
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
