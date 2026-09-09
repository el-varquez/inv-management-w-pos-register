namespace POS.Register.Types;

public record PaymentMethodDto(
    Guid Id,
    string Name,
    bool RequiresReference,
    bool IsActive,
    bool IsSystem);

public static class KnownPaymentMethods
{
    public static readonly Guid Cash = Guid.Parse("00000000-0000-0000-0000-000000000001");
}
