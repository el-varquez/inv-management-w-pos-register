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
    bool IsInvoice)
{
    public string Status => Refunded ? "Refunded" : "Completed";

    public static SaleRow From(SaleDto sale, Func<Guid?, string?> sukiNameOf)
    {
        var isInvoice = sale.MethodType == "Invoice";
        return new(
            sale.Id,
            sale.ReceiptNumber,
            sale.CreatedAt.ToLocalTime().ToString("h:mm tt"),
            sale.ItemCount,
            isInvoice
                ? $"{sale.PaymentMethod} · {sukiNameOf(sale.SukiId) ?? ""}"
                : sale.PaymentMethod,
            sale.Total,
            sale.IsRefunded,
            isInvoice);
    }
}

public record SaleLine(string Name, int Qty, decimal Total)
{
    public static SaleLine From(SaleLineDto line)
        => new(line.ItemName, line.Quantity, line.Total);
}
