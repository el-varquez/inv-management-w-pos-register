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
    Guid PaymentMethodId,
    string PaymentMethod,
    string MethodType,
    string? ReferenceNumber,
    decimal AmountTendered,
    decimal Change,
    bool IsRefunded,
    Guid? SukiId,
    List<SaleLineDto> Lines,
    DateTime CreatedAt);
