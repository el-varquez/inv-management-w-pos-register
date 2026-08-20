namespace POS.Register.Types;

public record PopularItemDto(
    Guid Id,
    string Name,
    string? Barcode,
    string ItemCode,
    decimal Price,
    int Stock,
    bool IsComposite,
    bool TracksStock,
    int QuantitySold);
