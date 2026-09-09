using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Features.Sell.Screens;
using POS.Register.Lib;
using POS.Register.Services;
using SharedComp = POS.Register.Components;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Sell.Components;

public partial class DetailModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => CloseCommand;
    public System.Windows.Input.ICommand DismissCommand => CloseCommand;

    private readonly AppShell.ShellViewModel _shell;

    public SaleRow Row { get; }
    public IReadOnlyList<SaleLine> Lines { get; }
    public string TotalDisplay => Peso.Format(Row.Amount);
    public string TimeAndPay => Row.Time + " · " + Row.Payment;

    public DetailModalViewModel(
        AppShell.ShellViewModel shell, SaleRow row, IReadOnlyList<SaleLine> lines)
    {
        _shell = shell;
        Row = row;
        Lines = lines;
    }

    [RelayCommand]
    private void Refund()
    {
        _shell.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            _shell,
            "Refund sale " + Row.Receipt,
            token => _ = CompleteRefundAsync(token)));
    }

    private async Task CompleteRefundAsync(string adminToken)
    {
        try
        {
            await _shell.SalesApi.RefundAsync(Row.Id, adminToken);
            _shell.OpenModal(new DetailModalViewModel(
                _shell, Row with { Refunded = true }, Lines));
            _shell.ShowToast("Sale " + Row.Receipt + " refunded — stock restored, money returned");
            _ = _shell.Sell.LoadListAsync();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void Close() => _shell.CloseModal();
}
