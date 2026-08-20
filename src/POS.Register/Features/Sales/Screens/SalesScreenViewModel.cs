using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Services;
using AppShell = POS.Register.Shell;
using SalesComp = POS.Register.Features.Sales.Components;

namespace POS.Register.Features.Sales.Screens;

public partial class SalesScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TxCount), nameof(IsEmpty))]
    private IReadOnlyList<SaleRow> rows = [];

    public string TxCount => Rows.Count == 1 ? "1 transaction" : $"{Rows.Count} transactions";

    public bool IsEmpty => Rows.Count == 0;

    public SalesScreenViewModel(AppShell.ShellViewModel shell) => ShellVm = shell;

    public async Task LoadAsync()
    {
        try
        {
            var sales = await ShellVm.SalesApi.GetTodayAsync();
            Rows = sales
                .Where(s => s.RefundedFromId is null)
                .Select(SaleRow.From)
                .ToList();
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private async Task OpenDetailAsync(SaleRow row)
    {
        try
        {
            var detail = await ShellVm.SalesApi.GetDetailAsync(row.Id);
            var lines = detail.Lines.Select(SaleLine.From).ToList();
            ShellVm.OpenModal(new SalesComp.DetailModalViewModel(
                ShellVm, row with { Refunded = detail.IsRefunded }, lines));
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }
}
