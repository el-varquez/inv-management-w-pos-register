using System.Windows.Controls;

namespace POS.Register.Components;

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
