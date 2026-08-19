using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;
using SharedComp = POS.Register.Components;
using ShiftComp = POS.Register.Features.Shifts.Components;

namespace POS.Register.Features.Shifts.Screens;

public partial class ShiftScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    public IReadOnlyList<Movement> Movements => CannedDay.Movements;
    public string OpenedAt => CannedDay.OpenedAt;
    public string StartingCashDisplay => Peso.Format(CannedDay.StartingCash);
    public string NetSalesDisplay => Peso.Format(CannedDay.NetSales);
    public string ExpectedCashDisplay => Peso.Format(CannedDay.ExpectedCash);
    public string ClosedSub => CannedDay.ClosedSub;
    public string StoreName => CannedDay.StoreName;

    public ShiftScreenViewModel(AppShell.ShellViewModel shell)
    {
        ShellVm = shell;
        ShellVm.Day.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ReceiptTitle));
            OnPropertyChanged(nameof(ReceiptCaption));
            OnPropertyChanged(nameof(ReceiptRows));
        };
    }

    public string ReceiptTitle => ShellVm.Day.IsClosed ? CannedDay.ZReadTitle : CannedDay.XReadTitle;
    public string ReceiptCaption => ShellVm.Day.IsClosed ? CannedDay.ZReadCaption : CannedDay.XReadCaption;
    public IReadOnlyList<ReceiptRow> ReceiptRows => ShellVm.Day.IsClosed ? CannedDay.ZReceiptRows : CannedDay.XReceiptRows;

    [RelayCommand]
    private void OpenStartingCash() => ShellVm.OpenModal(new SharedComp.StartingCashModalViewModel(ShellVm));

    [RelayCommand]
    private void OpenMovement() => ShellVm.OpenModal(new ShiftComp.MovementModalViewModel(ShellVm));

    [RelayCommand]
    private void XRead() => ShellVm.ShowToast("X read generated — live in the preview panel");

    [RelayCommand]
    private void OpenZRead() => ShellVm.OpenModal(new ShiftComp.ZReadModalViewModel(ShellVm));
}
