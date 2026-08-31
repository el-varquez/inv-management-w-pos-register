using System.Windows.Controls;
using System.Windows.Input;

namespace POS.Register.Components;

public partial class PaymentModalView : UserControl
{
    public PaymentModalView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is not PaymentModalViewModel vm)
            {
                return;
            }
            if (vm.IsCash)
            {
                TenderedBox.Focus();
                TenderedBox.SelectAll();
            }
            else if (vm.IsInvoice)
            {
                SearchBox.Focus();
            }
        };
    }

    private void OnDownTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsAsciiDigit);
    }
}
