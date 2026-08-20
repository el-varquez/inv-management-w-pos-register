namespace POS.Register.Types;

public record CreateSaleResult(
    Guid TransactionId,
    string ReceiptNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    decimal AmountTendered,
    decimal Change);
