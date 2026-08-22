using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;
using AppShell = POS.Register.Shell;
using AppStore = POS.Register.Store;
using SharedComp = POS.Register.Components;
using SellComp = POS.Register.Features.Sell.Components;

namespace POS.Register.Features.Sell.Screens;

public partial class SellScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    public event Action? ScanFocusRequested;

    private readonly DispatcherTimer _searchTimer;
    private bool _wasShiftOpen;

    public ObservableCollection<ResultRow> Results { get; } = [];
    public ObservableCollection<PopularTile> Popular { get; } = [];

    public IReadOnlyList<CartLine> Lines => ShellVm.Cart.Lines;
    public string ItemCountDisplay
        => ShellVm.Cart.ItemCount == 1 ? "1 item" : $"{ShellVm.Cart.ItemCount} items";
    public string LastRungName => ShellVm.Cart.LastRung is { } rung
        ? rung.Qty > 1 ? $"{rung.Qty}× {rung.Name}" : rung.Name
        : "";
    public string LastRungTotal
        => ShellVm.Cart.LastRung is { } rung ? Peso.Format(rung.Total) : "";
    public string SubtotalDisplay => Peso.Format(ShellVm.Cart.Subtotal);
    public string DiscountDisplay => ShellVm.Cart.Discount > 0
        ? "−" + Peso.Format(ShellVm.Cart.Discount)
        : Peso.Format(0m);
    public string TotalDisplay => Peso.Format(ShellVm.Cart.Total);
    public string DiscountButtonLabel => ShellVm.Cart.Discount > 0
        ? $"Discount · {Peso.Format(ShellVm.Cart.Discount)}"
        : "Discount";
    public string HotkeyHint => CannedDay.HotkeyHint;
    public string LockedTitle => ShellVm.Day.IsStoreClosedToday ? "Store closed for today" : CannedDay.LockedTitle;
    public string LockedBody => ShellVm.Day.IsStoreClosedToday
        ? $"Day #{ShellVm.Day.LatestClosedDay?.Number}'s Z read is done — the register opens again after midnight."
        : $"No starting cash, no transactions. Declare the drawer's starting cash to open shift #{ShellVm.Day.NextNumber}.";
    public bool CanOpenShift => !ShellVm.Day.IsStoreClosedToday;
    public bool ShowEWallet => ShellVm.Settings.TrackEWalletFloat;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQuery))]
    private string scanText = "";

    [ObservableProperty]
    private int selectedIndex = -1;

    [ObservableProperty]
    private int resultIndex = -1;

    public bool HasQuery => ScanText.Length > 0;
    public bool ShowResults => Results.Count > 0;

    public SellScreenViewModel(AppShell.ShellViewModel shell)
    {
        ShellVm = shell;
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _ = RunSearchAsync(ScanText.Trim());
        };
        ShellVm.Settings.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ShowEWallet));
        ShellVm.Day.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(LockedTitle));
            OnPropertyChanged(nameof(LockedBody));
            OnPropertyChanged(nameof(CanOpenShift));
            if (_wasShiftOpen && !ShellVm.Day.IsShiftOpen)
            {
                _ = RefreshPopularAsync();
            }
            _wasShiftOpen = ShellVm.Day.IsShiftOpen;
        };
        ShellVm.Cart.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ItemCountDisplay));
            OnPropertyChanged(nameof(LastRungName));
            OnPropertyChanged(nameof(LastRungTotal));
            OnPropertyChanged(nameof(SubtotalDisplay));
            OnPropertyChanged(nameof(DiscountDisplay));
            OnPropertyChanged(nameof(TotalDisplay));
            OnPropertyChanged(nameof(DiscountButtonLabel));
        };
    }

    public void RequestScanFocus() => ScanFocusRequested?.Invoke();

    public void ClearSearch()
    {
        _searchTimer.Stop();
        ScanText = "";
        Results.Clear();
        ResultIndex = -1;
        OnPropertyChanged(nameof(ShowResults));
    }

    partial void OnScanTextChanged(string value)
    {
        _searchTimer.Stop();
        if (value.Trim().Length == 0)
        {
            Results.Clear();
            ResultIndex = -1;
            OnPropertyChanged(nameof(ShowResults));
            return;
        }
        _searchTimer.Start();
    }

    private async Task RunSearchAsync(string term)
    {
        if (term.Length == 0 || term != ScanText.Trim())
        {
            return;
        }
        try
        {
            var items = await ShellVm.SellApi.SearchAsync(term);
            if (term != ScanText.Trim())
            {
                return;
            }
            Results.Clear();
            foreach (var item in items)
            {
                Results.Add(ResultRow.From(item));
            }
            ResultIndex = -1;
            OnPropertyChanged(nameof(ShowResults));
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    public async Task CommitSearchAsync()
    {
        var term = ScanText.Trim();
        if (term.Length == 0)
        {
            return;
        }
        if (ResultIndex >= 0 && ResultIndex < Results.Count)
        {
            var row = Results[ResultIndex];
            if (!row.OutOfStock)
            {
                Ring(row.Item);
            }
            return;
        }
        try
        {
            var items = await ShellVm.SellApi.SearchAsync(term);
            var exact = items.FirstOrDefault(i => i.Barcode == term || i.ItemCode == term)
                ?? (items.Count == 1 ? items[0] : null);
            if (exact is not null)
            {
                Ring(exact);
            }
        }
        catch (ApiException ex)
        {
            ShellVm.ShowToast(ex.Message);
        }
    }

    private void Ring(SellableItemDto item)
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            ShellVm.ShowToast("No starting cash, no transactions — open a shift first");
            return;
        }
        switch (ShellVm.Cart.TryAdd(item))
        {
            case AppStore.CartAddResult.OutOfStock:
                ShellVm.ShowToast($"{item.Name} is out of stock");
                return;
            case AppStore.CartAddResult.CapReached:
                ShellVm.ShowToast($"Only {item.Stock} available");
                return;
        }
        SelectedIndex = Lines.ToList().FindIndex(l => l.ItemId == item.Id);
        ClearSearch();
        RequestScanFocus();
    }

    public async Task RefreshPopularAsync()
    {
        try
        {
            var items = await ShellVm.SellApi.GetPopularAsync();
            Popular.Clear();
            foreach (var item in items)
            {
                Popular.Add(PopularTile.From(item));
            }
        }
        catch (ApiException)
        {
        }
    }

    [RelayCommand]
    private void AddPopular(PopularTile tile)
    {
        if (tile.IsOut)
        {
            return;
        }
        if (!ShellVm.Day.IsShiftOpen)
        {
            ShellVm.ShowToast("No starting cash, no transactions — open a shift first");
            return;
        }
        switch (ShellVm.Cart.TryAdd(tile.Item))
        {
            case AppStore.CartAddResult.OutOfStock:
                ShellVm.ShowToast($"{tile.Item.Name} is out of stock");
                return;
            case AppStore.CartAddResult.CapReached:
                ShellVm.ShowToast($"Only {tile.Item.Stock} available");
                return;
        }
        SelectedIndex = Lines.ToList().FindIndex(l => l.ItemId == tile.Item.Id);
    }

    [RelayCommand]
    private void IncrementLine(CartLine line)
    {
        if (!ShellVm.Cart.TrySetQty(line, line.Qty + 1))
        {
            ShellVm.ShowToast($"Only {line.Cap} available");
        }
    }

    [RelayCommand]
    private void DecrementLine(CartLine line) => ShellVm.Cart.TrySetQty(line, line.Qty - 1);

    [RelayCommand]
    private void ClearSale()
    {
        ShellVm.Cart.Clear();
        SelectedIndex = -1;
    }

    [RelayCommand]
    private void OpenPayment()
    {
        if (ShellVm.Cart.IsEmpty || !ShellVm.Day.IsShiftOpen)
        {
            ShellVm.ShowToast("Nothing to pay — ring an item first");
            return;
        }
        ShellVm.OpenModal(new SharedComp.PaymentModalViewModel(ShellVm));
    }

    [RelayCommand]
    private void OpenDiscount()
    {
        if (ShellVm.Cart.IsEmpty)
        {
            return;
        }
        ShellVm.OpenModal(new SellComp.DiscountModalViewModel(ShellVm));
    }

    [RelayCommand]
    private void OpenStartingCash()
    {
        if (ShellVm.Day.IsStoreClosedToday)
        {
            ShellVm.ShowToast(LockedBody);
            return;
        }
        ShellVm.OpenModal(new SharedComp.StartingCashModalViewModel(ShellVm));
    }

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
        if (Lines.Count == 0)
        {
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
        if (Lines.Count == 0)
        {
            return;
        }
        SelectedIndex = SelectedIndex < 0 ? Lines.Count - 1 : Math.Max(0, SelectedIndex - 1);
    }

    [RelayCommand]
    private void EditQty()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Lines.Count)
        {
            ShellVm.ShowToast("Select a line first — ↑/↓ arrows");
            return;
        }
        ShellVm.OpenModal(new SellComp.QtyModalViewModel(ShellVm, Lines[SelectedIndex]));
    }

    [RelayCommand]
    private void VoidLine()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Lines.Count)
        {
            ShellVm.ShowToast("Select a line first — ↑/↓ arrows");
            return;
        }
        var line = Lines[SelectedIndex];
        ShellVm.OpenModal(new SharedComp.AdminOverrideModalViewModel(
            ShellVm,
            $"Void line: {line.Name} ({line.Qty}×)",
            _ =>
            {
                ShellVm.Cart.Remove(line);
                SelectedIndex = Lines.Count == 0 ? -1 : Math.Min(SelectedIndex, Lines.Count - 1);
                ShellVm.ShowToast($"Line voided — {line.Name}");
            }));
    }

    [RelayCommand]
    private void Utang() => ShellVm.ShowToast("Utang is OFF — collection-only");

    [RelayCommand]
    private void EWallet()
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            return;
        }
        ShellVm.OpenModal(new SellComp.EWalletModalViewModel(ShellVm));
    }
}
