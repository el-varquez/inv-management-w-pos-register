using System.Windows;
using System.Windows.Controls;

namespace POS.Register.Features.Auth.Screens;

public partial class LoginScreenView : UserControl
{
    public LoginScreenView()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
