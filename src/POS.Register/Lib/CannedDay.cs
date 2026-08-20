namespace POS.Register.Lib;

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
}
