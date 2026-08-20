using System.Windows.Controls;

namespace POS.Register.Components;

public partial class DayCloseModalView : UserControl
{
    public DayCloseModalView()
    {
        InitializeComponent();
    }

    private void OnAdminPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DayCloseModalViewModel vm)
        {
            vm.AdminPassword = AdminPasswordBox.Password;
            vm.RequestClearPassword -= ClearPassword;
            vm.RequestClearPassword += ClearPassword;
        }
    }

    private void ClearPassword() => AdminPasswordBox.Password = "";
}
