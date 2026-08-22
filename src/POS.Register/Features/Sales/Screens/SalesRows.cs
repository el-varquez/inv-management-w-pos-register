using POS.Register.Types;

namespace POS.Register.Features.Sales.Screens;

public record SaleRow(
    Guid Id,
    string Receipt,
    string Time,
    int Items,
    string Payment,
    decimal Amount,
    bool Refunded,
    bool IsUtang)
{
    public string Status => Refunded ? "Refunded" : "Completed";

    public static SaleRow From(SaleDto sale, Func<Guid?, string?> sukiNameOf)
    {
        var isUtang = sale.PaymentType == "Utang";
        return new(
            sale.Id,
            sale.ReceiptNumber,
            sale.CreatedAt.ToLocalTime().ToString("h:mm tt"),
            sale.ItemCount,
            isUtang
                ? $"Utang · {sukiNameOf(sale.SukiId) ?? ""}"
                : MethodLabel(sale.PaymentType),
            sale.Total,
            sale.IsRefunded,
            isUtang);
    }

    private static string MethodLabel(string paymentType)
        => paymentType == "Gcash" ? "GCash" : paymentType;
}

public record SaleLine(string Name, int Qty, decimal Total)
{
    public static SaleLine From(SaleLineDto line)
        => new(line.ItemName, line.Quantity, line.Total);
}
