using POS.Register.Types;

namespace POS.Register.Lib;

public record ResultRow(
    SellableItemDto Item,
    string Code,
    string Name,
    string Barcode,
    string StockCaption,
    decimal Price,
    bool OutOfStock)
{
    public static ResultRow From(SellableItemDto item) => new(
        item,
        item.ItemCode,
        item.Name,
        item.Barcode is { Length: > 0 } barcode ? barcode : "no barcode",
        CaptionOf(item),
        item.Price,
        IsOut(item));

    public static string CaptionOf(SellableItemDto item)
        => !item.TracksStock && !item.IsComposite
            ? ""
            : item.IsComposite
                ? item.Stock > 0 ? $"makes {item.Stock}" : "Out"
                : item.Stock > 0 ? $"Stock {item.Stock}" : "Out of stock";

    public static bool IsOut(SellableItemDto item)
        => (item.TracksStock || item.IsComposite) && item.Stock <= 0;
}

public record PopularTile(
    PopularItemDto Item,
    string Name,
    decimal Price,
    string SoldCaption,
    bool IsOut,
    double Opacity)
{
    public static PopularTile From(PopularItemDto item)
    {
        var isOut = (item.TracksStock || item.IsComposite) && item.Stock <= 0;
        return new PopularTile(item, item.Name, item.Price, $"· {item.QuantitySold} sold", isOut, isOut ? 0.45 : 1.0);
    }
}
