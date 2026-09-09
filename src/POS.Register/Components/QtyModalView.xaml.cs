using System.Windows.Controls;

namespace POS.Register.Components;

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
