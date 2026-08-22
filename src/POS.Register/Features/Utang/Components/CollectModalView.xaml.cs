using System.Windows.Controls;
using System.Windows.Input;

namespace POS.Register.Features.Utang.Components;

public partial class CollectModalView : UserControl
{
    public CollectModalView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            AmountBox.Focus();
            AmountBox.SelectAll();
        };
    }

    private void OnAmountTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(c => char.IsAsciiDigit(c) || c == '.');
    }
}
