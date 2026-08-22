namespace POS.Register.Types;

public record UtangLedgerEntryDto(
    Guid Id,
    string Type,
    decimal Amount,
    decimal Markup,
    Guid? TransactionId,
    string? ReceiptNumber,
    string? Note,
    bool IsVoided,
    decimal? EditedFrom,
    DateTime CreatedAt);

public record SukiLedgerDto(
    Guid Id,
    string Name,
    string? Phone,
    decimal Balance,
    decimal MarkupEarned,
    List<UtangLedgerEntryDto> Entries);
