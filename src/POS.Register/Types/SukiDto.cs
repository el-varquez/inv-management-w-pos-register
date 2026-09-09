namespace POS.Register.Types;

public record SukiDto(
    Guid Id,
    string Name,
    string? Phone,
    decimal Balance,
    DateTime? DebtSince,
    DateTime? LastPaidAt,
    int? DaysSincePayment,
    bool PaymentOverdue);
