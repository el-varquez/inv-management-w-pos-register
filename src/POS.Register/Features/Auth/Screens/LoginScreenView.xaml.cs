using System.Windows;
using System.Windows.Controls;

namespace POS.Register.Features.Auth.Screens;

public partial class LoginScreenView : UserControl
{
    public LoginScreenView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LoginScreenViewModel oldVm)
        {
            oldVm.RequestClearPasswords -= ClearPasswordBoxes;
        }
        if (e.NewValue is LoginScreenViewModel newVm)
        {
            newVm.RequestClearPasswords += ClearPasswordBoxes;
        }
    }

    private void ClearPasswordBoxes()
    {
        PasswordBoxField.Clear();
        NewPasswordBox.Clear();
        ConfirmPasswordBox.Clear();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginScreenViewModel vm)
        {
            vm.Password = PasswordBoxField.Password;
        }
    }

    private void OnNewPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginScreenViewModel vm)
        {
            vm.NewPassword = NewPasswordBox.Password;
        }
    }

    private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginScreenViewModel vm)
        {
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
