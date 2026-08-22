using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using POS.Register.Types;
using AppShell = POS.Register.Shell;
using UtangScreens = POS.Register.Features.Utang.Screens;

namespace POS.Register.Features.Utang.Components;

public partial class CollectModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => CollectCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;
    private readonly UtangScreens.UtangScreenViewModel _screen;
    private readonly SukiDto _suki;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverBalance), nameof(AmountTone))]
    [NotifyCanExecuteChangedFor(nameof(CollectCommand))]
    private string amountText = "";

    [ObservableProperty]
    private bool isBusy;

    public string Title => $"Collect payment — {_suki.Name}";
    public string BalanceDisplay => Peso.Format(_suki.Balance);

    private decimal Amount =>
        decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value : 0m;

    public bool IsOverBalance => Amount > _suki.Balance;
    public string AmountTone => IsOverBalance ? "Red" : "Ink";

    public CollectModalViewModel(
        AppShell.ShellViewModel shell, UtangScreens.UtangScreenViewModel screen, SukiDto suki)
    {
        _shell = shell;
        _screen = screen;
        _suki = suki;
    }

    partial void OnAmountTextChanged(string value)
    {
        var cleaned = new string(value.Where(c => char.IsAsciiDigit(c) || c == '.').ToArray());
        if (cleaned != value)
        {
            AmountText = cleaned;
        }
    }

    [RelayCommand]
    private void Digit(string key) => AmountText += key;

    [RelayCommand]
    private void Backspace() => AmountText = AmountText.Length > 0 ? AmountText[..^1] : "";

    [RelayCommand]
    private void FullBalance()
        => AmountText = _suki.Balance.ToString("0.##", CultureInfo.InvariantCulture);

    private bool CanCollect() => !IsBusy && Amount > 0m && !IsOverBalance;

    [RelayCommand(CanExecute = nameof(CanCollect))]
    private async Task CollectAsync()
    {
        if (IsBusy)
        {
            return;
        }
        IsBusy = true;
        CollectCommand.NotifyCanExecuteChanged();
        try
        {
            var amount = Amount;
            await _shell.UtangApi.CollectAsync(_suki.Id, amount);
            _shell.CloseModal();
            _shell.ShowToast($"{Peso.Format(amount)} collected from {_suki.Name} — cash in drawer");
            await _screen.RefreshAfterActionAsync();
        }
        catch (ApiException ex)
        {
            _shell.ShowToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
            CollectCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.CloseModal();
}
