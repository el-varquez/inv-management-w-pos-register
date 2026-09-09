using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Store;
using AppShell = POS.Register.Shell;
using InvoiceComp = POS.Register.Features.Invoices.Components;

namespace POS.Register.Features.Invoices.Screens;

public partial class InvoiceScreenViewModel : AppShell.TicketScreenViewModel
{
    public override CartStore Cart => ShellVm.InvoiceCart;
    public override string PanelTitle => "Current invoice";
    public override string CommitLabel => "CHARGE — F5";
    public override string HotkeyHint => CannedDay.InvoiceHotkeyHint;
    protected override string EmptyCommitToast => "Nothing to charge — ring an item first";

    public decimal MarkupTotal => Cart.Lines
        .Sum(l => (l.UtangMarkup ?? ShellVm.Settings.DefaultUtangMarkup) * l.Qty);
    public string MarkupDisplay => "+" + Peso.Format(MarkupTotal);
    public override string TotalDisplay => Peso.Format(Cart.Subtotal + MarkupTotal - Cart.Discount);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InvoiceCount), nameof(IsEmpty))]
    private IReadOnlyList<InvoiceRow> rows = [];

    public string InvoiceCount => Rows.Count == 1 ? "1 invoice" : $"{Rows.Count} invoices";

    public bool IsEmpty => Rows.Count == 0;

    public InvoiceScreenViewModel(AppShell.ShellViewModel shell) : base(shell)
    {
    }

    protected override void OpenCommit()
        => ShellVm.OpenModal(new InvoiceComp.ChargeModalViewModel(ShellVm));

    protected override void RaiseTotals()
    {
        base.RaiseTotals();
        OnPropertyChanged(nameof(MarkupDisplay));
    }

    public override async Task LoadListAsync()
    {
        try
        {
            var invoices = await ShellVm.InvoiceApi.GetTodayAsync();
            Rows = invoices.Select(InvoiceRow.From).ToList();
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    [RelayCommand]
    private async Task OpenDetailAsync(InvoiceRow row)
    {
        try
        {
            var detail = await ShellVm.InvoiceApi.GetDetailAsync(row.Id);
            var lines = detail.Lines.Select(InvoiceLine.From).ToList();
            ShellVm.OpenModal(new InvoiceComp.InvoiceDetailModalViewModel(
                ShellVm, row with { Voided = detail.IsVoided }, lines));
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }
}
