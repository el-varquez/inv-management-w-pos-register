using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using AppShell = POS.Register.Shell;
using SharedComp = POS.Register.Components;
using SellComp = POS.Register.Features.Sell.Components;

namespace POS.Register.Features.Sell.Screens;

public partial class SellScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    public event Action? ScanFocusRequested;

    public IReadOnlyList<CartLine> Lines => CannedDay.Cart;
    public IReadOnlyList<PopularItem> Popular => CannedDay.Popular;
    public IReadOnlyList<SearchResult> Results => CannedDay.Results;
    public string ItemCountDisplay => CannedDay.ItemCountDisplay;
    public string LastRungName => CannedDay.LastRungName;
    public string LastRungTotal => CannedDay.LastRungTotal;
    public string SubtotalDisplay => Peso.Format(CannedDay.Subtotal);
    public string DiscountDisplay => Peso.Format(CannedDay.Discount);
    public string TotalDisplay => Peso.Format(CannedDay.CartTotal);
    public string HotkeyHint => CannedDay.HotkeyHint;
    public string LockedTitle => CannedDay.LockedTitle;
    public string LockedBody => CannedDay.LockedBody;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowResults))]
    private string scanText = "";

    [ObservableProperty]
    private int selectedIndex = -1;

    [ObservableProperty]
    private int resultIndex = -1;

    public bool ShowResults => ScanText.Length > 0;

    public SellScreenViewModel(AppShell.ShellViewModel shell) => ShellVm = shell;

    public void RequestScanFocus() => ScanFocusRequested?.Invoke();

    public void ClearSearch()
    {
        ScanText = "";
        ResultIndex = -1;
    }

    [RelayCommand]
    private void OpenPayment()
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            ShellVm.ShowToast("Nothing to pay — ring an item first");
            return;
        }
        ShellVm.OpenModal(new SharedComp.PaymentModalViewModel(ShellVm));
    }

    [RelayCommand]
    private void OpenDiscount() => ShellVm.OpenModal(new SellComp.DiscountModalViewModel(ShellVm));

    [RelayCommand]
    private void OpenStartingCash() => ShellVm.OpenModal(new SharedComp.StartingCashModalViewModel(ShellVm));

    [RelayCommand]
    private void SelectNext()
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            return;
        }
        if (ShowResults)
        {
            ResultIndex = Math.Min(Results.Count - 1, ResultIndex + 1);
            return;
        }
        SelectedIndex = SelectedIndex < 0 ? 0 : Math.Min(Lines.Count - 1, SelectedIndex + 1);
    }

    [RelayCommand]
    private void SelectPrev()
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            return;
        }
        if (ShowResults)
        {
            ResultIndex = Math.Max(-1, ResultIndex - 1);
            return;
        }
        SelectedIndex = SelectedIndex < 0 ? Lines.Count - 1 : Math.Max(0, SelectedIndex - 1);
    }

    [RelayCommand]
    private void EditQty()
    {
        if (SelectedIndex < 0)
        {
            ShellVm.ShowToast("Select a line first — ↑/↓ arrows");
            return;
        }
        ShellVm.OpenModal(new SellComp.QtyModalViewModel(ShellVm, Lines[SelectedIndex]));
    }

    [RelayCommand]
    private void VoidLine()
    {
        if (SelectedIndex < 0)
        {
            ShellVm.ShowToast("Select a line first — ↑/↓ arrows");
            return;
        }
        var line = Lines[SelectedIndex];
        ShellVm.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            ShellVm,
            "Void line: " + line.Name + " (" + line.Qty + "×)",
            () => ShellVm.ShowToast("Line voided — " + line.Name)));
    }

    [RelayCommand]
    private void Utang() => ShellVm.ShowToast("Utang is OFF — collection-only");
}
