using System.Windows.Controls;
using System.Windows.Input;

namespace POS.Register.Features.Shifts.Components;

public partial class XReadModalView : UserControl
{
    public XReadModalView()
    {
        InitializeComponent();
    }

    private void OnWalletTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsAsciiDigit);
    }
}
