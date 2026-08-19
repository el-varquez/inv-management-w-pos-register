using System.Globalization;

namespace POS.Register.Lib;

public static class Peso
{
    public static string Format(decimal amount) =>
        string.Format(CultureInfo.InvariantCulture, "₱{0:N2}", amount);

    public static string Signed(decimal amount) =>
        amount < 0 ? "-" + Format(-amount) : "+" + Format(amount);
}
