using System.Windows.Controls;
using System.Windows.Input;

namespace POS.Register.Features.Sell.Components;

public partial class UtangChargeModalView : UserControl
{
    public UtangChargeModalView()
    {
        InitializeComponent();
        Loaded += (_, _) => SearchBox.Focus();
    }

    private void OnDownTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsAsciiDigit);
    }
}
