using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Features.Invoices.Screens;
using POS.Register.Lib;
using POS.Register.Services;
using SharedComp = POS.Register.Components;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Invoices.Components;

public partial class InvoiceDetailModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public System.Windows.Input.ICommand DefaultCommand => CloseCommand;
    public System.Windows.Input.ICommand DismissCommand => CloseCommand;

    private readonly AppShell.ShellViewModel _shell;

    public InvoiceRow Row { get; }
    public IReadOnlyList<InvoiceLine> Lines { get; }
    public string TotalDisplay => Peso.Format(Row.Amount);
    public string TimeAndSuki => Row.Time + " · " + Row.Suki;

    public InvoiceDetailModalViewModel(
        AppShell.ShellViewModel shell, InvoiceRow row, IReadOnlyList<InvoiceLine> lines)
    {
        _shell = shell;
        Row = row;
        Lines = lines;
    }

    [RelayCommand]
    private void Void()
    {
        _shell.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            _shell,
            "Void invoice " + Row.Number,
            token => _ = CompleteVoidAsync(token)));
    }

    private async Task CompleteVoidAsync(string adminToken)
    {
        try
        {
            await _shell.InvoiceApi.VoidAsync(Row.Id, adminToken);
            _shell.OpenModal(new InvoiceDetailModalViewModel(
                _shell, Row with { Voided = true }, Lines));
            _shell.ShowToast("Invoice " + Row.Number + " voided — stock restored");
            _ = _shell.Invoices.LoadListAsync();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void Close() => _shell.CloseModal();
}
