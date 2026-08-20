using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Services;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class AdminOverrideModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly Action<string> _onApproved;

    public string Reason { get; }

    public event Action? RequestClearPassword;

    [ObservableProperty]
    private string adminUsername = "";

    [ObservableProperty]
    private string adminPassword = "";

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isBusy;

    public AdminOverrideModalViewModel(AppShell.ShellViewModel shell, string reason, Action<string> onApproved)
    {
        _shell = shell;
        _onApproved = onApproved;
        Reason = reason;
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy)
        {
            return;
        }
        if (AdminUsername.Trim().Length == 0 || AdminPassword.Length == 0)
        {
            ErrorMessage = "Ask an admin to enter their username and password.";
            return;
        }
        IsBusy = true;
        try
        {
            var login = await _shell.AuthenticateAsync(AdminUsername.Trim(), AdminPassword);
            if (login.PasswordSetupRequired || login.Token is null || login.Role != "Admin")
            {
                ErrorMessage = "Those credentials don't belong to an admin account.";
                RequestClearPassword?.Invoke();
                return;
            }
            _shell.CloseModal();
            _onApproved(login.Token!);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            RequestClearPassword?.Invoke();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
