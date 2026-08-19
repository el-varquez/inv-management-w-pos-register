using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Features.Auth.Services;
using POS.Register.Services;
using POS.Register.Types;
using AppShell = POS.Register.Shell;
using AppStore = POS.Register.Store;

namespace POS.Register.Features.Auth.Screens;

public partial class LoginScreenViewModel : ObservableObject
{
    public const string StageCredentials = "Credentials";
    public const string StageSetPassword = "SetPassword";
    public const string StageAdminRedirect = "AdminRedirect";

    private readonly AppShell.ShellViewModel _shell;
    private readonly AuthService _auth;

    public AppStore.SettingsStore Settings { get; }

    public string DateLong => DateTime.Now.ToString("dddd, MMMM d, yyyy");

    [ObservableProperty]
    private string stage = StageCredentials;

    [ObservableProperty]
    private string username = "";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string newPassword = "";

    [ObservableProperty]
    private string confirmPassword = "";

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isBusy;

    public event Action? RequestClearPasswords;

    public LoginScreenViewModel(
        AppShell.ShellViewModel shell, AuthService auth, AppStore.SettingsStore settings)
    {
        _shell = shell;
        _auth = auth;
        Settings = settings;
    }

    [RelayCommand]
    private async Task LogInAsync()
    {
        if (IsBusy)
        {
            return;
        }
        ErrorMessage = null;
        if (Username.Trim().Length == 0 || Password.Length == 0)
        {
            ErrorMessage = "Enter your username and password.";
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _auth.LoginAsync(Username.Trim(), Password);
            if (result.PasswordSetupRequired)
            {
                Stage = result.Role == "Admin" ? StageAdminRedirect : StageSetPassword;
                return;
            }
            Complete(result);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SetPasswordAsync()
    {
        if (IsBusy)
        {
            return;
        }
        ErrorMessage = null;
        if (NewPassword.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords don't match.";
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _auth.SetupPasswordAsync(Username.Trim(), NewPassword);
            Complete(result);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void BackToLogIn()
    {
        Stage = StageCredentials;
        ErrorMessage = null;
        ClearSecrets();
    }

    public void Submit()
    {
        if (Stage == StageSetPassword)
        {
            if (SetPasswordCommand.CanExecute(null))
            {
                SetPasswordCommand.Execute(null);
            }
            return;
        }
        if (Stage == StageAdminRedirect)
        {
            BackToLogIn();
            return;
        }
        if (LogInCommand.CanExecute(null))
        {
            LogInCommand.Execute(null);
        }
    }

    public void Reset()
    {
        Stage = StageCredentials;
        Username = "";
        ErrorMessage = null;
        ClearSecrets();
    }

    private void Complete(LoginResponse result)
    {
        ClearSecrets();
        ErrorMessage = null;
        Stage = StageCredentials;
        Username = "";
        _shell.EnterSession(result);
    }

    private void ClearSecrets()
    {
        Password = "";
        NewPassword = "";
        ConfirmPassword = "";
        RequestClearPasswords?.Invoke();
    }
}
