using System.Windows.Controls;

namespace POS.Register.Features.Invoices.Components;

public partial class ChargeModalView : UserControl
{
    public ChargeModalView()
    {
        InitializeComponent();
        Loaded += (_, _) => SearchBox.Focus();
    }
}
