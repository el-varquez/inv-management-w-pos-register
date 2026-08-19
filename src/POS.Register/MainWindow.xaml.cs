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
            else if (shell.IsLoggedIn && ReferenceEquals(shell.CurrentScreen, shell.Sell) && shell.Sell.ShowResults)
            {
                shell.Sell.ClearSearch();
                e.Handled = true;
            }
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
                shell.NavigateCommand.Execute("Sell");
                if (shell.Day.IsShiftOpen)
                {
                    shell.Sell.RequestScanFocus();
                }
                e.Handled = true;
            }
            return;
        }
        if (!shell.IsLoggedIn || shell.ActiveModal is not null || !ReferenceEquals(shell.CurrentScreen, shell.Sell))
        {
            return;
        }
        switch (e.Key)
        {
            case Key.F2:
                shell.Sell.EditQtyCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F5:
                shell.Sell.OpenPaymentCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F6:
                shell.Sell.VoidLineCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F7:
                shell.Sell.UtangCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Down:
                shell.Sell.SelectNextCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Up:
                shell.Sell.SelectPrevCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
