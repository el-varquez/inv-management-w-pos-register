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
        };
        foreach (var m in read.MethodSales)
        {
            rows.Add(new(m.Name, Peso.Format(m.Amount)));
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
        return rows;
    }
}
