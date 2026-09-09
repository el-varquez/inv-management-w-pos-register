using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Store;
using AppShell = POS.Register.Shell;
using SellComp = POS.Register.Features.Sell.Components;
using SharedComp = POS.Register.Components;

namespace POS.Register.Features.Sell.Screens;

public partial class SellScreenViewModel : AppShell.TicketScreenViewModel
{
    public override CartStore Cart => ShellVm.Cart;
    public override string PanelTitle => "Current sale";
    public override string CommitLabel => "PAYMENT — F5";
    public override string HotkeyHint => CannedDay.HotkeyHint;
    protected override string EmptyCommitToast => "Nothing to pay — ring an item first";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TxCount), nameof(IsEmpty))]
    private IReadOnlyList<SaleRow> rows = [];

    public string TxCount => Rows.Count == 1 ? "1 transaction" : $"{Rows.Count} transactions";

    public bool IsEmpty => Rows.Count == 0;

    public SellScreenViewModel(AppShell.ShellViewModel shell) : base(shell)
    {
    }

    protected override void OpenCommit()
        => ShellVm.OpenModal(new SharedComp.PaymentModalViewModel(ShellVm));

    public override async Task LoadListAsync()
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
            ShellVm.OpenModal(new SellComp.DetailModalViewModel(
                ShellVm, row with { Refunded = detail.IsRefunded }, lines));
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private void OpenPaymentPicker()
    {
        OpenPaymentCommand.Execute(null);
        if (ShellVm.ActiveModal is SharedComp.PaymentModalViewModel modal)
        {
            modal.ToggleMethodCommand.Execute(null);
        }
    }
}
