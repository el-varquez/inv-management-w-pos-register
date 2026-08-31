namespace POS.Register.Types;

public record DayReadDto(
    Guid Id,
    int Number,
    bool IsClosed,
    bool ClosedLate,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal NetSales,
    int TransactionCount,
    List<MethodSalesDto> MethodSales,
    decimal DrawerMovementsNet,
    decimal? CountedCash,
    decimal? CashVariance,
    decimal? CountedEWalletBalance,
    decimal? EWalletVariance,
    int ShiftCount,
    List<ShiftSummaryDto> Shifts);

public record DaySummaryDto(
    Guid Id,
    int Number,
    bool IsClosed,
    bool ClosedLate,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal? NetSales,
    decimal? CountedCash,
    decimal? CashVariance,
    int ShiftCount);
