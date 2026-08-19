using System.Windows.Controls;

namespace POS.Register.Features.Sell.Screens;

public partial class SellScreenView : UserControl
{
    public SellScreenView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is SellScreenViewModel oldVm)
            {
                oldVm.ScanFocusRequested -= FocusScan;
            }
            if (e.NewValue is SellScreenViewModel newVm)
            {
                newVm.ScanFocusRequested += FocusScan;
            }
        };
    }

    private void FocusScan() => ScanBox.Focus();
}
