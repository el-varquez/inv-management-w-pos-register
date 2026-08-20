namespace POS.Register.Lib;

public record CartLine(string Code, string Barcode, string Name, int Qty, decimal Price)
{
    public decimal Total => Qty * Price;
}

public record PopularItem(string Name, decimal Price, string SoldCaption);

public record SearchResult(string Code, string Name, string Barcode, string StockCaption, decimal Price, bool OutOfStock);

public record SaleRow(string Receipt, string Time, int Items, string Payment, decimal Amount, bool Refunded)
{
    public string Status => Refunded ? "Refunded" : "Completed";
}

public record SaleLine(string Name, int Qty, decimal Price)
{
    public decimal Total => Qty * Price;
}

public record Movement(string Note, decimal Amount, bool IsPayout, bool IsVoided = false)
{
    public string AmountDisplay => Peso.Signed(Amount);
}

public record ReceiptRow(string Label, string Value = "", bool IsHead = false, string Tone = "Ink", bool Bold = false, bool TopBorder = false);

public static class CannedDay
{
    public const string StoreName = "Aling Nena's Store";

    public const string LockedTitle = "No starting cash declared";

    public static readonly IReadOnlyList<CartLine> Cart =
    [
        new("1003", "4807770190162", "Lucky Me Pancit Canton Original", 3, 18m),
        new("1001", "4801981126712", "Coke Mismo 300ml", 2, 25m),
        new("1007", "4800016641503", "Sky Flakes Crackers 25g", 1, 12m),
    ];

    public const string ItemCountDisplay = "6 items";
    public const string LastRungName = "Sky Flakes Crackers 25g";
    public const string LastRungTotal = "₱12.00";
    public const decimal Subtotal = 116m;
    public const decimal Discount = 0m;
    public const decimal CartTotal = 116m;
    public const string TenderedDisplay = "200";
    public const string ChangeDisplay = "₱84.00";
    public const string SuccessReceipt = "Receipt R-20260818-0016 · Cash";

    public static readonly IReadOnlyList<string> QuickBills = ["EXACT", "₱200", "₱500", "₱1,000"];

    public const string HotkeyHint = "F1 search · ↑/↓ select line · F2 edit qty · F6 void line · F5 payment · F7 utang · Esc close";

    public static readonly IReadOnlyList<PopularItem> Popular =
    [
        new("Lucky Me Pancit Canton", 18m, "· 142 sold"),
        new("Coke Mismo 300ml", 25m, "· 98 sold"),
        new("Kopiko Blanca Twin", 12m, "· 91 sold"),
        new("Sky Flakes 25g", 12m, "· 77 sold"),
        new("Nescafé 3-in-1", 10m, "· 74 sold"),
        new("Piattos Cheese 40g", 22m, "· 58 sold"),
        new("C2 Apple 355ml", 20m, "· 55 sold"),
        new("Hansel Mocha", 8m, "· 49 sold"),
    ];

    public static readonly IReadOnlyList<SearchResult> Results =
    [
        new("1001", "Coke Mismo 300ml", "4801981126712", "Stock 46", 25m, false),
        new("1031", "Coke 1L", "4801981145301", "Stock 12", 65m, false),
        new("1058", "Coke Zero Mismo 300ml", "4801981119906", "Out of stock", 28m, true),
    ];

    public static readonly IReadOnlyList<SaleRow> Sales =
    [
        new("R-20260818-0001", "7:14 AM", 4, "Cash", 116m, false),
        new("R-20260818-0002", "7:31 AM", 7, "Cash", 342m, false),
        new("R-20260818-0003", "8:05 AM", 2, "Cash", 89m, false),
        new("R-20260818-0004", "8:22 AM", 9, "Cash", 455m, false),
        new("R-20260818-0005", "9:10 AM", 5, "Cash", 230m, false),
        new("R-20260818-0006", "9:36 AM", 2, "Cash", 65m, true),
        new("R-20260818-0007", "10:02 AM", 6, "Cash", 785m, false),
        new("R-20260818-0008", "10:48 AM", 8, "Cash", 512m, false),
        new("R-20260818-0009", "11:15 AM", 3, "GCash", 250m, false),
        new("R-20260818-0010", "12:07 PM", 2, "Cash", 96m, false),
        new("R-20260818-0011", "1:26 PM", 10, "Cash", 610m, false),
        new("R-20260818-0012", "2:11 PM", 4, "Maya", 180m, false),
        new("R-20260818-0013", "2:59 PM", 6, "Cash", 340m, false),
        new("R-20260818-0014", "3:44 PM", 11, "Cash", 660m, false),
        new("R-20260818-0015", "4:48 PM", 7, "Cash", 495m, false),
    ];

    public const string TxCount = "15 transactions";

    public static readonly IReadOnlyList<SaleLine> DetailLines =
    [
        new("Bear Brand Powdered Milk 320g", 1, 490m),
        new("Milo Powder 300g", 1, 235m),
        new("Eggs per pc", 5, 12m),
    ];

}
