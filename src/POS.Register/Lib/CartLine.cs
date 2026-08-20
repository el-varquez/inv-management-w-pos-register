using CommunityToolkit.Mvvm.ComponentModel;

namespace POS.Register.Lib;

public partial class CartLine : ObservableObject
{
    public Guid ItemId { get; }
    public string Code { get; }
    public string Barcode { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Cap { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    private int qty;

    public decimal Total => Qty * Price;

    public CartLine(Guid itemId, string code, string barcode, string name, decimal price, int cap, int qty)
    {
        ItemId = itemId;
        Code = code;
        Barcode = barcode;
        Name = name;
        Price = price;
        Cap = cap;
        Qty = qty;
    }
}
