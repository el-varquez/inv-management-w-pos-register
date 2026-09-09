using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using POS.Register.Shell;

namespace POS.Register;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ShellViewModel shell)
            {
                shell.PropertyChanged += OnShellPropertyChanged;
            }
        };
    }

    private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellViewModel.ActiveModal) && sender is ShellViewModel { ActiveModal: not null })
        {
            ModalHost.Focus();
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (DataContext is not ShellViewModel shell)
        {
            return;
        }
        if (e.Key == Key.Escape)
        {
            if (shell.ActiveModal is not null)
            {
                shell.DismissCommand.Execute(null);
                e.Handled = true;
            }
            else if (shell.IsLoggedIn && shell.ActiveTicket is { HasQuery: true } queried)
            {
                queried.ClearSearch();
                e.Handled = true;
            }
            return;
        }
        if (e.Key is Key.Down or Key.Up
            && shell.ActiveModal is Features.Invoices.Components.ChargeModalViewModel { Rows.Count: > 0 } chargeModal)
        {
            if (e.Key == Key.Down)
            {
                chargeModal.SelectNextCommand.Execute(null);
            }
            else
            {
                chargeModal.SelectPrevCommand.Execute(null);
            }
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F7 && shell.ActiveModal is Components.PaymentModalViewModel paymentModal)
        {
            paymentModal.ToggleMethodCommand.Execute(null);
            e.Handled = true;
            return;
        }
        if (e.Key is Key.Enter or Key.Return)
        {
            if (Keyboard.FocusedElement is ButtonBase)
            {
                return;
            }
            shell.DefaultCommand.Execute(null);
            e.Handled = true;
            return;
        }
        if (e.Key == Key.F1)
        {
            if (shell.IsLoggedIn && shell.ActiveModal is null)
            {
                if (shell.ActiveTicket is { } current)
                {
                    current.FocusSearch();
                }
                else
                {
                    shell.NavigateCommand.Execute("Sell");
                }
                e.Handled = true;
            }
            return;
        }
        if (!shell.IsLoggedIn || shell.ActiveModal is not null || shell.ActiveTicket is not { } screen)
        {
            return;
        }
        switch (e.Key)
        {
            case Key.F2:
                screen.EditQtyCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F5:
                screen.OpenPaymentCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F6:
                screen.VoidLineCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F7:
                if (screen is Features.Sell.Screens.SellScreenViewModel sellScreen)
                {
                    sellScreen.OpenPaymentPickerCommand.Execute(null);
                    e.Handled = true;
                }
                break;
            case Key.Down:
                screen.SelectNextCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Up:
                screen.SelectPrevCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
