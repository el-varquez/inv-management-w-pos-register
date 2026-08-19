using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using SharedComp = POS.Register.Components;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Sales.Components;

public partial class DetailModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => CloseCommand;
    public System.Windows.Input.ICommand DismissCommand => CloseCommand;

    private readonly AppShell.ShellViewModel _shell;

    public SaleRow Row { get; }
    public IReadOnlyList<SaleLine> Lines => CannedDay.DetailLines;
    public string TotalDisplay => Peso.Format(Row.Amount);
    public string TimeAndPay => Row.Time + " · " + Row.Payment;

    public DetailModalViewModel(AppShell.ShellViewModel shell, SaleRow row)
    {
        _shell = shell;
        Row = row;
    }

    [RelayCommand]
    private void Refund()
    {
        var receipt = Row.Receipt;
        _shell.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            _shell,
            "Refund sale " + receipt,
            () => _shell.ShowToast("Sale " + receipt + " refunded — stock restored, money returned")));
    }

    [RelayCommand]
    private void Close() => _shell.CloseModal();
}
