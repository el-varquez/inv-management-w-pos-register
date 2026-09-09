using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Components;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Store;
using POS.Register.Types;

namespace POS.Register.Shell;

public abstract partial class TicketScreenViewModel : ObservableObject
{
    public ShellViewModel ShellVm { get; }

    public event Action? ScanFocusRequested;

    private readonly DispatcherTimer _searchTimer;
    private bool _wasShiftOpen;

    public ObservableCollection<ResultRow> Results { get; } = [];
    public ObservableCollection<PopularTile> Popular { get; } = [];

    public abstract CartStore Cart { get; }
    public abstract string PanelTitle { get; }
    public abstract string CommitLabel { get; }
    public abstract string HotkeyHint { get; }
    protected abstract string EmptyCommitToast { get; }
    protected abstract void OpenCommit();

    public IReadOnlyList<CartLine> Lines => Cart.Lines;
    public string ItemCountDisplay => Cart.ItemCount == 1 ? "1 item" : $"{Cart.ItemCount} items";
    public string LastRungName => Cart.LastRung is { } rung
        ? rung.Qty > 1 ? $"{rung.Qty}× {rung.Name}" : rung.Name
        : "";
    public string LastRungTotal => Cart.LastRung is { } rung ? Peso.Format(rung.Total) : "";
    public string SubtotalDisplay => Peso.Format(Cart.Subtotal);
    public string DiscountDisplay => Cart.Discount > 0
        ? "−" + Peso.Format(Cart.Discount)
        : Peso.Format(0m);
    public virtual string TotalDisplay => Peso.Format(Cart.Total);
    public string DiscountButtonLabel => Cart.Discount > 0
        ? $"Discount · {Peso.Format(Cart.Discount)}"
        : "Discount";
    public string LockedTitle => ShellVm.Day.IsStoreClosedToday ? "Store closed for today" : CannedDay.LockedTitle;
    public string LockedBody => ShellVm.Day.IsStoreClosedToday
        ? $"Day #{ShellVm.Day.LatestClosedDay?.Number}'s Z read is done — the register opens again after midnight."
        : $"No starting cash, no transactions. Declare the drawer's starting cash to open shift #{ShellVm.Day.NextNumber}.";
    public bool CanOpenShift => !ShellVm.Day.IsStoreClosedToday;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQuery))]
    private string scanText = "";

    [ObservableProperty]
    private int selectedIndex = -1;

    [ObservableProperty]
    private int resultIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowingRing))]
    private bool showingList;

    public bool ShowingRing => !ShowingList;

    public bool HasQuery => ScanText.Length > 0;
    public bool ShowResults => Results.Count > 0;

    public abstract Task LoadListAsync();

    protected TicketScreenViewModel(ShellViewModel shell)
    {
        ShellVm = shell;
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _ = RunSearchAsync(ScanText.Trim());
        };
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
        Cart.PropertyChanged += (_, _) => RaiseTotals();
    }

    protected virtual void RaiseTotals()
    {
        OnPropertyChanged(nameof(ItemCountDisplay));
        OnPropertyChanged(nameof(LastRungName));
        OnPropertyChanged(nameof(LastRungTotal));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(DiscountDisplay));
        OnPropertyChanged(nameof(TotalDisplay));
        OnPropertyChanged(nameof(DiscountButtonLabel));
    }

    public void RequestScanFocus() => ScanFocusRequested?.Invoke();

    public void FocusScanIfPossible()
    {
        if (ShellVm.Day.IsShiftOpen && !ShowingList)
        {
            RequestScanFocus();
        }
    }

    public void FocusSearch()
    {
        ShowingList = false;
        FocusScanIfPossible();
    }

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
            var items = await ShellVm.Catalog.SearchAsync(term);
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
            var items = await ShellVm.Catalog.SearchAsync(term);
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
        switch (Cart.TryAdd(item))
        {
            case CartAddResult.OutOfStock:
                ShellVm.ShowToast($"{item.Name} is out of stock");
                return;
            case CartAddResult.CapReached:
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
            var items = await ShellVm.Catalog.GetPopularAsync();
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
        switch (Cart.TryAdd(tile.Item))
        {
            case CartAddResult.OutOfStock:
                ShellVm.ShowToast($"{tile.Item.Name} is out of stock");
                return;
            case CartAddResult.CapReached:
                ShellVm.ShowToast($"Only {tile.Item.Stock} available");
                return;
        }
        SelectedIndex = Lines.ToList().FindIndex(l => l.ItemId == tile.Item.Id);
    }

    [RelayCommand]
    private void IncrementLine(CartLine line)
    {
        if (!Cart.TrySetQty(line, line.Qty + 1))
        {
            ShellVm.ShowToast($"Only {line.Cap} available");
        }
    }

    [RelayCommand]
    private void DecrementLine(CartLine line) => Cart.TrySetQty(line, line.Qty - 1);

    [RelayCommand]
    private void ShowRing() => FocusSearch();

    [RelayCommand]
    private void ShowList()
    {
        ShowingList = true;
        _ = LoadListAsync();
    }

    [RelayCommand]
    private void ClearSale()
    {
        Cart.Clear();
        SelectedIndex = -1;
    }

    [RelayCommand]
    private void OpenPayment()
    {
        if (Cart.IsEmpty || !ShellVm.Day.IsShiftOpen)
        {
            ShellVm.ShowToast(EmptyCommitToast);
            return;
        }
        OpenCommit();
    }

    [RelayCommand]
    private void OpenDiscount()
    {
        if (Cart.IsEmpty)
        {
            return;
        }
        ShellVm.OpenModal(new DiscountModalViewModel(ShellVm, Cart));
    }

    [RelayCommand]
    private void OpenStartingCash()
    {
        if (ShellVm.Day.IsStoreClosedToday)
        {
            ShellVm.ShowToast(LockedBody);
            return;
        }
        ShellVm.OpenModal(new StartingCashModalViewModel(ShellVm));
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
        ShellVm.OpenModal(new QtyModalViewModel(ShellVm, Cart, Lines[SelectedIndex]));
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
        ShellVm.OpenModal(new AdminOverrideModalViewModel(
            ShellVm,
            $"Void line: {line.Name} ({line.Qty}×)",
            _ =>
            {
                Cart.Remove(line);
                SelectedIndex = Lines.Count == 0 ? -1 : Math.Min(SelectedIndex, Lines.Count - 1);
                ShellVm.ShowToast($"Line voided — {line.Name}");
            }));
    }
}
