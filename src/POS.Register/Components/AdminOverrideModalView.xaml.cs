using System.Windows.Controls;

namespace POS.Register.Components;

public partial class AdminOverrideModalView : UserControl
{
    public AdminOverrideModalView()
    {
        InitializeComponent();
        Loaded += (_, _) => AdminUsernameBox.Focus();
    }

    private void OnAdminPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is AdminOverrideModalViewModel vm)
        {
            vm.AdminPassword = AdminPasswordBox.Password;
            vm.RequestClearPassword -= ClearPassword;
            vm.RequestClearPassword += ClearPassword;
        }
    }

    private void ClearPassword() => AdminPasswordBox.Password = "";
}
