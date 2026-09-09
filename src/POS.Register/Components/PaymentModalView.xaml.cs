using System.Windows.Controls;

namespace POS.Register.Components;

public partial class PaymentModalView : UserControl
{
    public PaymentModalView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is PaymentModalViewModel { IsCash: true })
            {
                TenderedBox.Focus();
                TenderedBox.SelectAll();
            }
        };
    }
}
