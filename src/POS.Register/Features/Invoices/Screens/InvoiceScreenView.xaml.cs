using System.Windows.Controls;

namespace POS.Register.Features.Invoices.Screens;

public partial class InvoiceScreenView : UserControl
{
    public InvoiceScreenView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is InvoiceScreenViewModel oldVm)
            {
                oldVm.ScanFocusRequested -= FocusScan;
            }
            if (e.NewValue is InvoiceScreenViewModel newVm)
            {
                newVm.ScanFocusRequested += FocusScan;
            }
        };
    }

    private void FocusScan() => ScanBox.Focus();
}
