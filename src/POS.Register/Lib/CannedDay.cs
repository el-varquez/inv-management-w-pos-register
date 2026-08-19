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

public record Movement(string Note, decimal Amount, bool IsPayout)
{
    public string AmountDisplay => Peso.Signed(Amount);
}

public record DenominationCount(string Label, int Count, decimal Value)
{
    public decimal Total => Count * Value;
}

public record ReceiptRow(string Label, string Value = "", bool IsHead = false, string Tone = "Ink", bool Bold = false, bool TopBorder = false);

public static class CannedDay
{
    public const string StoreName = "Aling Nena's Store";
    public const string CashierName = "Marites";
    public const int ShiftNumber = 12;
    public const int PreviousShiftNumber = 11;
    public const int NextShiftNumber = 13;
    public const string OpenedAt = "7:02 AM";
    public const decimal StartingCash = 2000m;
    public const decimal NetSales = 5160m;
    public const decimal ExpectedCash = 6230m;
    public const decimal CountedCash = 6215m;

    public const string OpenChip = "Shift #12 · Open 7:02 AM";
    public const string FreshChip = "CLOSED · SHIFT #11";
    public const string ClosedChip = "CLOSED · SHIFT #12";

    public const string LockedTitle = "No starting cash declared";
    public const string LockedBody = "No starting cash, no transactions. Declare the drawer's starting cash to open shift #12.";

    public const string ClosedSub = "Closed with ₱6,215.00 counted · short by ₱15.00 · Expected ₱6,230.00";

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

    public static readonly IReadOnlyList<Movement> Movements =
    [
        new("Rema drinks delivery", -1000m, true),
        new("Change fund from owner", 500m, false),
    ];

    public const string PayoutExpectedAfter = "₱5,230.00";
    public const string PayInExpectedAfter = "₱6,730.00";
    public const string MovementAmount = "1,000";
    public const string MovementNote = "Rema drinks delivery";

    public static readonly IReadOnlyList<DenominationCount> Denominations =
    [
        new("₱1000", 5, 1000m),
        new("₱500", 2, 500m),
        new("₱200", 0, 200m),
        new("₱100", 1, 100m),
        new("₱50", 1, 50m),
        new("₱20", 3, 20m),
        new("Coins", 1, 5m),
    ];

    public const string ZVerdict = "SHORT by ₱15.00";

    public const string XReadTitle = "X READ — SHIFT #12";
    public const string XReadCaption = "Mid-shift · live preview";
    public const string ZReadTitle = "Z READ #12 — SHIFT #12";
    public const string ZReadCaption = "Closeout report · final";

    private static readonly IReadOnlyList<ReceiptRow> SharedRows =
    [
        new("Sales (14 paid txns)", "₱5,160.00"),
        new("Refunds (1) — excluded", "₱65.00", Tone: "Red"),
        new("Net sales (paid)", "₱5,160.00", Bold: true, TopBorder: true),
        new("BY PAYMENT", IsHead: true),
        new("Cash", "₱4,730.00"),
        new("GCash", "₱250.00"),
        new("Maya", "₱180.00"),
        new("DRAWER", IsHead: true),
        new("Starting cash", "₱2,000.00"),
        new("Payouts / pay-ins", "-₱500.00", Tone: "Red"),
        new("Expected cash in drawer", "₱6,230.00", Bold: true, TopBorder: true),
    ];

    public static readonly IReadOnlyList<ReceiptRow> XReceiptRows = SharedRows;

    public static readonly IReadOnlyList<ReceiptRow> ZReceiptRows =
    [
        .. SharedRows,
        new("Counted cash", "₱6,215.00"),
        new("Short", "₱15.00", Tone: "Red", Bold: true),
    ];
}
