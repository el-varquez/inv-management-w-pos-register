using System.Windows.Controls;
using System.Windows.Input;

namespace POS.Register.Features.Sell.Components;

public partial class EWalletModalView : UserControl
{
    public EWalletModalView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            AmountBox.Focus();
            AmountBox.SelectAll();
        };
    }

    private void OnMoneyTextInput(object sender, TextCompositionEventArgs e)
    {
        var box = (TextBox)sender;
        e.Handled = !e.Text.All(c => char.IsAsciiDigit(c) || c == '.')
            || (e.Text.Contains('.') && box.Text.Contains('.'));
    }

    private void OnMoneyFocus(object sender, KeyboardFocusChangedEventArgs e)
        => ((TextBox)sender).SelectAll();
}
