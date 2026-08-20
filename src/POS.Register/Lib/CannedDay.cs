namespace POS.Register.Lib;

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

    public const string HotkeyHint = "F1 search · ↑/↓ select line · F2 edit qty · F6 void line · F5 payment · F7 utang · Esc close";

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
