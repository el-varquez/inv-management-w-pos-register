using System.Windows.Controls;

namespace POS.Register.Features.Sell.Components;

public partial class QtyModalView : UserControl
{
    public QtyModalView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            QtyBox.Focus();
            QtyBox.SelectAll();
        };
    }
}
