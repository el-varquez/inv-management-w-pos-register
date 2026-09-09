using POS.Register.Types;

namespace POS.Register.Features.Invoices.Screens;

public record InvoiceRow(
    Guid Id,
    string Number,
    string Time,
    string Suki,
    decimal Amount,
    bool Voided)
{
    public string Status => Voided ? "Voided" : "Charged";

    public static InvoiceRow From(InvoiceDto invoice)
        => new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CreatedAt.ToLocalTime().ToString("h:mm tt"),
            invoice.SukiName,
            invoice.Total,
            invoice.IsVoided);
}

public record InvoiceLine(string Name, int Qty, decimal Total)
{
    public static InvoiceLine From(InvoiceLineDto line)
        => new(line.ItemName, line.Quantity, line.Total);
}
