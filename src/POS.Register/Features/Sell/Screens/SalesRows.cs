using POS.Register.Types;

namespace POS.Register.Features.Sell.Screens;

public record SaleRow(
    Guid Id,
    string Receipt,
    string Time,
    int Items,
    string Payment,
    decimal Amount,
    bool Refunded)
{
    public string Status => Refunded ? "Refunded" : "Completed";

    public static SaleRow From(SaleDto sale)
        => new(
            sale.Id,
            sale.ReceiptNumber,
            sale.CreatedAt.ToLocalTime().ToString("h:mm tt"),
            sale.ItemCount,
            sale.PaymentMethod,
            sale.Total,
            sale.IsRefunded);
}

public record SaleLine(string Name, int Qty, decimal Total)
{
    public static SaleLine From(SaleLineDto line)
        => new(line.ItemName, line.Quantity, line.Total);
}
