using System.Windows.Controls;
using System.Windows.Input;

namespace POS.Register.Components;

public partial class StartingCashModalView : UserControl
{
    public StartingCashModalView()
    {
        InitializeComponent();
    }

    private void OnAmountTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsAsciiDigit);
    }
}
