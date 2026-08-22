namespace POS.Register.Types;

public record SellableItemDto(
    Guid Id,
    string Name,
    string? Barcode,
    string ItemCode,
    decimal Price,
    decimal? UtangMarkup,
    int Stock,
    bool IsComposite,
    bool TracksStock);
