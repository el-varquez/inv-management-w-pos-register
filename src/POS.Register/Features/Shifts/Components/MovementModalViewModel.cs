using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Shifts.Components;

public partial class MovementModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => ConfirmCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public MoneyEntry Amount { get; } = new();

    [ObservableProperty]
    private string note = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpectedAfterDisplay))]
    private bool isPayout = true;

    [ObservableProperty]
    private bool isBusy;

    public string ExpectedAfterDisplay
        => Peso.Format((_shell.Day.Current?.ExpectedCash ?? 0m) + SignedAmount);

    private decimal SignedAmount => IsPayout ? -Amount.Value : Amount.Value;

    public MovementModalViewModel(AppShell.ShellViewModel shell)
    {
        _shell = shell;
        Amount.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ExpectedAfterDisplay));
    }

    [RelayCommand]
    private void SetPayout() => IsPayout = true;

    [RelayCommand]
    private void SetPayIn() => IsPayout = false;

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy)
        {
            return;
        }
        if (Amount.Value <= 0m)
        {
            _shell.ShowToast("Enter an amount.");
            return;
        }
        if (Note.Trim().Length == 0)
        {
            _shell.ShowToast("A note is required — record what the money was for.");
            return;
        }
        IsBusy = true;
        try
        {
            await _shell.ShiftApi.RecordMovementAsync(SignedAmount, Note.Trim());
            await _shell.RefreshShiftAsync();
            _shell.CloseModal();
            _shell.ShowToast(IsPayout ? "Payout recorded" : "Pay-in recorded");
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
