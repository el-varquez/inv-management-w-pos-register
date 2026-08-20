using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace POS.Register.Lib;

public partial class MoneyEntry : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Value), nameof(Display))]
    private string digits = "";

    public decimal Value =>
        Digits.Length == 0 ? 0m : decimal.Parse(Digits, CultureInfo.InvariantCulture);

    public string Display => Value.ToString("N0", CultureInfo.InvariantCulture);

    public void Push(string keys)
    {
        var next = (Digits + keys).TrimStart('0');
        if (next.Length > 7)
        {
            return;
        }
        Digits = next;
    }

    public void Backspace()
    {
        if (Digits.Length > 0)
        {
            Digits = Digits[..^1];
        }
    }

    public void Clear() => Digits = "";
}
