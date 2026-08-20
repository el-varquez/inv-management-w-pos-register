using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using POS.Register.Lib;
using POS.Register.Types;

namespace POS.Register.Store;

public enum CartAddResult
{
    Added,
    OutOfStock,
    CapReached
}

public partial class CartStore : ObservableObject
{
    public ObservableCollection<CartLine> Lines { get; } = [];

    [ObservableProperty]
    private decimal requestedDiscount;

    [ObservableProperty]
    private CartLine? lastRung;

    public decimal Subtotal => Lines.Sum(l => l.Total);
    public decimal Discount => Math.Min(RequestedDiscount, Subtotal);
    public decimal Total => Subtotal - Discount;
    public int ItemCount => Lines.Sum(l => l.Qty);
    public bool IsEmpty => Lines.Count == 0;

    public CartAddResult TryAdd(Guid itemId, string itemCode, string? barcode, string name, decimal price, int stock, bool tracksStock, bool isComposite)
    {
        var cap = tracksStock || isComposite ? stock : int.MaxValue;
        var line = Lines.FirstOrDefault(l => l.ItemId == itemId);
        if (line is null)
        {
            if (cap <= 0)
            {
                return CartAddResult.OutOfStock;
            }
            line = new CartLine(itemId, itemCode, barcode ?? "", name, price, cap, 1);
            line.PropertyChanged += (_, _) => RaiseTotals();
            Lines.Add(line);
        }
        else
        {
            if (line.Qty >= line.Cap)
            {
                return CartAddResult.CapReached;
            }
            line.Qty++;
        }
        LastRung = line;
        RaiseTotals();
        return CartAddResult.Added;
    }

    public CartAddResult TryAdd(SellableItemDto item)
        => TryAdd(item.Id, item.ItemCode, item.Barcode, item.Name, item.Price,
            item.Stock, item.TracksStock, item.IsComposite);

    public CartAddResult TryAdd(PopularItemDto item)
        => TryAdd(item.Id, item.ItemCode, item.Barcode, item.Name, item.Price,
            item.Stock, item.TracksStock, item.IsComposite);

    public bool TrySetQty(CartLine line, int qty)
    {
        if (qty < 1 || qty > line.Cap)
        {
            return false;
        }
        line.Qty = qty;
        LastRung = line;
        RaiseTotals();
        return true;
    }

    public void Remove(CartLine line)
    {
        Lines.Remove(line);
        if (ReferenceEquals(LastRung, line))
        {
            LastRung = null;
        }
        RaiseTotals();
    }

    public void Clear()
    {
        Lines.Clear();
        RequestedDiscount = 0m;
        LastRung = null;
        RaiseTotals();
    }

    partial void OnRequestedDiscountChanged(decimal value) => RaiseTotals();

    private void RaiseTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Discount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(IsEmpty));
    }
}
