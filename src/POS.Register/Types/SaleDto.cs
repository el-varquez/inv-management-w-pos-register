namespace POS.Register.Types;

public record SaleDto(
    Guid Id,
    string ReceiptNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string PaymentType,
    decimal AmountTendered,
    decimal Change,
    bool IsRefunded,
    Guid? RefundedFromId,
    int ItemCount,
    DateTime CreatedAt);
