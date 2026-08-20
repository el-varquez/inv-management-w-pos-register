namespace POS.Register.Types;

public record ShiftSummaryDto(
    Guid Id,
    int Number,
    bool IsClosed,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal StartingCash,
    decimal? NetSales,
    decimal? ExpectedCash,
    decimal? CountedCash,
    decimal? CashVariance);
