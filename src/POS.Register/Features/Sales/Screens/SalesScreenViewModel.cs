using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;
using SalesComp = POS.Register.Features.Sales.Components;

namespace POS.Register.Features.Sales.Screens;

public partial class SalesScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    public IReadOnlyList<SaleRow> Rows => CannedDay.Sales;
    public string TxCount => CannedDay.TxCount;

    public SalesScreenViewModel(AppShell.ShellViewModel shell) => ShellVm = shell;

    [RelayCommand]
    private void OpenDetail(SaleRow row) =>
        ShellVm.OpenModal(new SalesComp.DetailModalViewModel(ShellVm, row));
}
