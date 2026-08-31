namespace POS.Register.Types;

public record MethodSalesDto(Guid PaymentMethodId, string Name, decimal Amount);

public record DrawerMovementDto(
    Guid Id,
    decimal Amount,
    string Note,
    bool IsVoided,
    DateTime CreatedAt);

public record ShiftReadDto(
    Guid Id,
    int Number,
    bool IsClosed,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal StartingCash,
    decimal? StartingCashOriginal,
    string? StartingCashCorrectionReason,
    decimal NetSales,
    int TransactionCount,
    decimal Refunds,
    int RefundCount,
    List<MethodSalesDto> MethodSales,
    int EWalletCashInCount,
    decimal EWalletCashIn,
    int EWalletCashOutCount,
    decimal EWalletCashOut,
    int UtangChargedCount,
    decimal UtangCharged,
    decimal UtangMarkup,
    decimal UtangCollections,
    decimal DrawerMovementsNet,
    decimal ExpectedCash,
    decimal? CountedCash,
    decimal? CountedCashOriginal,
    string? CorrectionReason,
    decimal? CashVariance,
    decimal? StartingEWalletBalance,
    decimal? ExpectedEWalletBalance,
    decimal? CountedEWalletBalance,
    decimal? EWalletVariance,
    List<DrawerMovementDto> Movements);
