using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Services;
using AppShell = POS.Register.Shell;

namespace POS.Register.Features.Shifts.Components;

public partial class DenominationRow : ObservableObject
{
    public string Label { get; }
    public decimal UnitValue { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    private int count;

    public decimal Total => Count * UnitValue;

    public DenominationRow(string label, decimal unitValue)
    {
        Label = label;
        UnitValue = unitValue;
    }
}

public partial class XReadModalViewModel : ObservableObject, AppShell.IDefaultAction
{
    public ICommand DefaultCommand => EndShiftCommand;
    public ICommand DismissCommand => CancelCommand;

    private readonly AppShell.ShellViewModel _shell;

    public ObservableCollection<DenominationRow> Denominations { get; } =
    [
        new("₱1,000", 1000m),
        new("₱500", 500m),
        new("₱200", 200m),
        new("₱100", 100m),
        new("₱50", 50m),
        new("₱20", 20m),
        new("₱10 · coin", 10m),
        new("₱5 · coin", 5m),
        new("₱1 · coin", 1m),
    ];

    public MoneyEntry WalletCounted { get; } = new();
    public bool ShowWallet { get; }

    public string Title => $"End shift #{_shell.Day.Current?.Number} — count the drawer";
    public decimal CountedTotal => Denominations.Sum(d => d.Total);
    public string CountedDisplay => Peso.Format(CountedTotal);
    public string ExpectedDisplay => Peso.Format(_shell.Day.Current?.ExpectedCash ?? 0m);
    public string WalletExpectedDisplay => Peso.Format(_shell.Day.Current?.ExpectedEWalletBalance ?? 0m);

    private decimal WalletVariance =>
        WalletCounted.Value - (_shell.Day.Current?.ExpectedEWalletBalance ?? 0m);

    public string WalletVerdict => WalletVariance switch
    {
        < 0 => $"WALLET SHORT by {Peso.Format(-WalletVariance)}",
        > 0 => $"WALLET OVER by {Peso.Format(WalletVariance)}",
        _ => "WALLET BALANCED — matches the app",
    };

    public string WalletVerdictTone => WalletVariance switch
    {
        < 0 => "Red",
        > 0 => "Gold",
        _ => "Confirm",
    };

    private decimal Variance => CountedTotal - (_shell.Day.Current?.ExpectedCash ?? 0m);

    public string Verdict => Variance switch
    {
        < 0 => $"SHORT by {Peso.Format(-Variance)}",
        > 0 => $"OVER by {Peso.Format(Variance)}",
        _ => "BALANCED — drawer matches expected",
    };

    public string VerdictTone => Variance switch
    {
        < 0 => "Red",
        > 0 => "Gold",
        _ => "Confirm",
    };

    [ObservableProperty]
    private bool isBusy;

    public XReadModalViewModel(AppShell.ShellViewModel shell)
    {
        _shell = shell;
        ShowWallet = shell.Settings.TrackEWalletFloat;
        WalletCounted.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(WalletVerdict));
            OnPropertyChanged(nameof(WalletVerdictTone));
        };
    }

    [RelayCommand]
    private void Increment(DenominationRow row)
    {
        row.Count++;
        RecountChanged();
    }

    [RelayCommand]
    private void Decrement(DenominationRow row)
    {
        if (row.Count > 0)
        {
            row.Count--;
            RecountChanged();
        }
    }

    private void RecountChanged()
    {
        OnPropertyChanged(nameof(CountedTotal));
        OnPropertyChanged(nameof(CountedDisplay));
        OnPropertyChanged(nameof(Verdict));
        OnPropertyChanged(nameof(VerdictTone));
    }

    [RelayCommand]
    private async Task EndShiftAsync()
    {
        if (IsBusy || _shell.Day.Current is not { } current)
        {
            return;
        }
        IsBusy = true;
        try
        {
            await _shell.ShiftApi.CloseAsync(current.Id, CountedTotal, ShowWallet ? WalletCounted.Value : null);
            await _shell.RefreshShiftAsync();
            _shell.CloseModal();
            _shell.ShowToast($"Shift #{current.Number} ended — X read saved");
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
