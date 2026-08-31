namespace POS.Register.Types;

public record SaleDto(
    Guid Id,
    string ReceiptNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    Guid PaymentMethodId,
    string PaymentMethod,
    string MethodType,
    decimal AmountTendered,
    decimal Change,
    bool IsRefunded,
    Guid? RefundedFromId,
    Guid? SukiId,
    int ItemCount,
    DateTime CreatedAt);
