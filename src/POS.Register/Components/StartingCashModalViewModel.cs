using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using AppShell = POS.Register.Shell;

namespace POS.Register.Components;

public partial class StartingCashModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public MoneyEntry Amount { get; } = new();
    public int NextNumber { get; }
    public string Body => $"Opens shift #{NextNumber}. Count the drawer before opening.";
    public string ConfirmText => $"OPEN SHIFT #{NextNumber}";

    [ObservableProperty]
    private bool isBusy;

    public StartingCashModalViewModel(AppShell.ShellViewModel shell)
    {
        _shell = shell;
        NextNumber = shell.Day.NextNumber;
    }

    [RelayCommand]
    private void Digit(string key) => Amount.Push(key);

    [RelayCommand]
    private void Backspace() => Amount.Backspace();

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy)
        {
            return;
        }
        if (Amount.Value <= 0m)
        {
            _shell.ShowToast("Starting cash must be greater than ₱0.");
            return;
        }
        IsBusy = true;
        try
        {
            await _shell.ShiftApi.OpenAsync(Amount.Value);
            await _shell.RefreshShiftAsync();
            _shell.CloseModal();
            _shell.ShowToast(
                $"Shift #{_shell.Day.Current?.Number} opened with {Peso.Format(Amount.Value)} starting cash");
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
