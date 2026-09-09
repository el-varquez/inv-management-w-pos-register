namespace POS.Register.Types;

public record CreateSaleResult(
    Guid SaleId,
    string ReceiptNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    decimal AmountTendered,
    decimal Change);

public record RefundResult(
    Guid RefundSaleId,
    string ReceiptNumber,
    decimal RefundedAmount);
