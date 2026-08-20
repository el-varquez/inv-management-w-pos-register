namespace POS.Register.Types;

public record SaleLineDto(
    string ItemName,
    decimal UnitPrice,
    int Quantity,
    decimal Discount,
    decimal Total);

public record SaleDetailDto(
    Guid Id,
    string ReceiptNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string PaymentType,
    string? ReferenceNumber,
    decimal AmountTendered,
    decimal Change,
    bool IsRefunded,
    List<SaleLineDto> Lines,
    DateTime CreatedAt);
