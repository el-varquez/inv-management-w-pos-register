using POS.Register.Lib;
using POS.Register.Types;

namespace POS.Register.Features.Shifts.Screens;

public static class DayReceipt
{
    public static IReadOnlyList<ReceiptRow> Rows(DayReadDto day)
    {
        var rows = new List<ReceiptRow>
        {
            new($"Sales ({day.TransactionCount} paid txns)", Peso.Format(day.NetSales)),
            new("Net sales (paid)", Peso.Format(day.NetSales), Bold: true, TopBorder: true),
            new("BY PAYMENT", IsHead: true),
        };
        foreach (var m in day.MethodSales)
        {
            rows.Add(new(m.Name, Peso.Format(m.Amount)));
        }
        rows.Add(new("DRAWER", IsHead: true));
        if (day.DrawerMovementsNet != 0m)
        {
            rows.Add(new("Payouts / pay-ins",
                day.DrawerMovementsNet < 0m
                    ? "-" + Peso.Format(-day.DrawerMovementsNet)
                    : Peso.Format(day.DrawerMovementsNet),
                Tone: day.DrawerMovementsNet < 0m ? "Red" : "Confirm"));
        }
        if (day.CountedCash is { } counted)
        {
            rows.Add(new("Counted cash (last X read)", Peso.Format(counted), Bold: true, TopBorder: true));
        }
        var variance = day.CashVariance ?? 0m;
        if (variance < 0m)
        {
            rows.Add(new("Short (all shifts)", Peso.Format(-variance), Tone: "Red", Bold: true));
        }
        else if (variance > 0m)
        {
            rows.Add(new("Over (all shifts)", Peso.Format(variance), Tone: "Gold", Bold: true));
        }
        else
        {
            rows.Add(new("Balanced", Peso.Format(0m), Tone: "Confirm", Bold: true));
        }
        if (day.CountedEWalletBalance is { } countedWallet)
        {
            rows.Add(new("E-WALLET — SECOND DRAWER", IsHead: true));
            rows.Add(new("Counted balance (last X read)", Peso.Format(countedWallet), Bold: true, TopBorder: true));
            var walletVariance = day.EWalletVariance ?? 0m;
            if (walletVariance < 0m)
            {
                rows.Add(new("Wallet short (all shifts)", Peso.Format(-walletVariance), Tone: "Red", Bold: true));
            }
            else if (walletVariance > 0m)
            {
                rows.Add(new("Wallet over (all shifts)", Peso.Format(walletVariance), Tone: "Gold", Bold: true));
            }
            else
            {
                rows.Add(new("Wallet balanced", Peso.Format(0m), Tone: "Confirm", Bold: true));
            }
        }
        rows.Add(new("SHIFTS", IsHead: true));
        foreach (var shift in day.Shifts.Where(s => s.IsClosed))
        {
            var v = shift.CashVariance ?? 0m;
            rows.Add(v < 0m
                ? new($"Shift #{shift.Number}", $"short {Peso.Format(-v)}", Tone: "Red")
                : v > 0m
                    ? new($"Shift #{shift.Number}", $"over {Peso.Format(v)}", Tone: "Gold")
                    : new($"Shift #{shift.Number}", "balanced", Tone: "Confirm"));
        }
        return rows;
    }
}
