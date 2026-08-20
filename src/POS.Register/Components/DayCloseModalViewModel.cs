using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class DayCloseModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly int _dayNumber;

    public bool NeedsAdmin { get; }
    public string Title => $"Close day #{_dayNumber} — Z read";
    public string Body { get; }

    public event Action? RequestClearPassword;

    [ObservableProperty]
    private string adminUsername = "";

    [ObservableProperty]
    private string adminPassword = "";

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isBusy;

    public DayCloseModalViewModel(AppShell.ShellViewModel shell)
    {
        _shell = shell;
        var day = shell.Day.CurrentDay;
        _dayNumber = day?.Number ?? 0;
        NeedsAdmin = shell.Session.Role != "Admin";
        var lastShift = day?.Shifts.LastOrDefault(s => s.IsClosed);
        Body = lastShift is not null
            ? $"The Z read takes shift #{lastShift.Number}'s counted {Peso.Format(lastShift.CountedCash ?? 0m)} as final — no recount. {day!.Shifts.Count(s => s.IsClosed)} shift(s) today."
            : "The Z read freezes the day's totals — no recount.";
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy)
        {
            return;
        }
        string? token = null;
        if (NeedsAdmin)
        {
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
                    IsBusy = false;
                    return;
                }
                token = login.Token;
            }
            catch (ApiException ex)
            {
                ErrorMessage = ex.Message;
                RequestClearPassword?.Invoke();
                IsBusy = false;
                return;
            }
        }
        IsBusy = true;
        try
        {
            if (token is null)
            {
                await _shell.DayApi.CloseAsync();
            }
            else
            {
                await _shell.DayApi.CloseAsync(token);
            }
            await _shell.RefreshShiftAsync();
            _shell.CloseModal();
            _shell.ShowToast($"Day #{_dayNumber} closed — Z read saved");
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
    private void Cancel() => _shell.CloseModal();
}
