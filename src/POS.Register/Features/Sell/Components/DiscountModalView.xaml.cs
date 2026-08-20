using System.Windows.Controls;

namespace POS.Register.Features.Sell.Components;

public partial class DiscountModalView : UserControl
{
    public DiscountModalView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            AmountBox.Focus();
            AmountBox.SelectAll();
        };
    }
}
