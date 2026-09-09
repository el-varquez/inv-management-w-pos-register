namespace POS.Register.Types;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid SukiId,
    string SukiName,
    decimal Total,
    bool IsVoided,
    DateTime CreatedAt,
    Guid CreatedBy);

public record InvoiceLineDto(
    string ItemName,
    decimal UnitPrice,
    int Quantity,
    decimal Discount,
    decimal Total);

public record InvoiceDetailDto(
    Guid Id,
    string InvoiceNumber,
    Guid SukiId,
    string SukiName,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal MarkupTotal,
    decimal Total,
    Guid ShiftId,
    bool IsVoided,
    DateTime? VoidedAt,
    Guid? VoidedBy,
    List<InvoiceLineDto> Lines,
    DateTime CreatedAt,
    Guid CreatedBy);

public record CreateInvoiceResult(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal MarkupTotal,
    decimal Total,
    decimal NewBalance);
