using POS.Register.Lib;
using POS.Register.Types;

namespace POS.Register.Features.Shifts.Screens;

public static class ShiftReceipt
{
    public static IReadOnlyList<ReceiptRow> Rows(ShiftReadDto read)
    {
        var rows = new List<ReceiptRow>
        {
            new($"Sales ({read.TransactionCount} paid txns)", Peso.Format(read.NetSales)),
            new($"Refunds ({read.RefundCount}) — excluded", Peso.Format(read.Refunds), Tone: "Red"),
            new("Net sales (paid)", Peso.Format(read.NetSales), Bold: true, TopBorder: true),
            new("BY PAYMENT", IsHead: true),
            new("Cash", Peso.Format(read.CashSales)),
            new("GCash", Peso.Format(read.GcashSales)),
            new("Maya", Peso.Format(read.MayaSales)),
            new("UTANG — NOT SALES", IsHead: true),
            new($"Charged on credit ({read.UtangChargedCount})", Peso.Format(read.UtangCharged)),
            new($"incl. {Peso.Format(read.UtangMarkup)} markup", Tone: "Ink3"),
            new("Collections (incl. down payments)", Peso.Format(read.UtangCollections)),
        };
        if (read.EWalletCashInCount > 0 || read.EWalletCashOutCount > 0)
        {
            rows.Add(new("E-WALLET — NOT SALES", IsHead: true));
            rows.Add(new($"Cash in ({read.EWalletCashInCount}) — sent from wallet",
                Peso.Format(read.EWalletCashIn)));
            rows.Add(new($"Cash out ({read.EWalletCashOutCount}) — received to wallet",
                Peso.Format(read.EWalletCashOut)));
            rows.Add(new("Fees earned are counted in sales above", Tone: "Ink3"));
        }
        rows.Add(new("DRAWER", IsHead: true));
        rows.Add(new("Starting cash", Peso.Format(read.StartingCash)));
        if (read.DrawerMovementsNet != 0m)
        {
            rows.Add(new("Payouts / pay-ins",
                read.DrawerMovementsNet < 0m
                    ? "-" + Peso.Format(-read.DrawerMovementsNet)
                    : Peso.Format(read.DrawerMovementsNet),
                Tone: read.DrawerMovementsNet < 0m ? "Red" : "Confirm"));
        }
        rows.Add(new("Expected cash in drawer", Peso.Format(read.ExpectedCash), Bold: true, TopBorder: true));
        if (read.IsClosed && read.CountedCash is { } counted)
        {
            rows.Add(new("Counted cash", Peso.Format(counted)));
            var variance = read.CashVariance ?? 0m;
            if (variance < 0m)
            {
                rows.Add(new("Short", Peso.Format(-variance), Tone: "Red", Bold: true));
            }
            else if (variance > 0m)
            {
                rows.Add(new("Over", Peso.Format(variance), Tone: "Gold", Bold: true));
            }
            else
            {
                rows.Add(new("Balanced", Peso.Format(0m), Tone: "Confirm", Bold: true));
            }
        }
        if (read.StartingEWalletBalance is { } startingWallet)
        {
            rows.Add(new("E-WALLET — SECOND DRAWER", IsHead: true));
            rows.Add(new("Starting balance", Peso.Format(startingWallet)));
            rows.Add(new("Expected balance", Peso.Format(read.ExpectedEWalletBalance ?? startingWallet), Bold: true, TopBorder: true));
            if (read.IsClosed && read.CountedEWalletBalance is { } countedWallet)
            {
                rows.Add(new("Counted balance", Peso.Format(countedWallet)));
                var walletVariance = read.EWalletVariance ?? 0m;
                if (walletVariance < 0m)
                {
                    rows.Add(new("Wallet short", Peso.Format(-walletVariance), Tone: "Red", Bold: true));
                }
                else if (walletVariance > 0m)
                {
                    rows.Add(new("Wallet over", Peso.Format(walletVariance), Tone: "Gold", Bold: true));
                }
                else
                {
                    rows.Add(new("Wallet balanced", Peso.Format(0m), Tone: "Confirm", Bold: true));
                }
            }
        }
        return rows;
    }
}
