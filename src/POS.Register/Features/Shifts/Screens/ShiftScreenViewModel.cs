using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Register.Lib;
using POS.Register.Types;
using AppShell = POS.Register.Shell;
using SharedComp = POS.Register.Components;
using ShiftComp = POS.Register.Features.Shifts.Components;

namespace POS.Register.Features.Shifts.Screens;

public partial class ShiftScreenViewModel : ObservableObject
{
    public AppShell.ShellViewModel ShellVm { get; }

    public ShiftScreenViewModel(AppShell.ShellViewModel shell)
    {
        ShellVm = shell;
        ShellVm.Day.PropertyChanged += (_, _) => OnPropertyChanged(string.Empty);
        ShellVm.Settings.PropertyChanged += (_, _) => OnPropertyChanged(nameof(StoreName));
    }

    public string StoreName => ShellVm.Settings.StoreName;

    public string OpenTitle => $"Shift #{ShellVm.Day.Current?.Number}";
    public string OpenedAt =>
        ShellVm.Day.Current is { } open ? open.OpenedAt.ToLocalTime().ToString("h:mm tt") : "";
    public string StartingCashDisplay => Peso.Format(ShellVm.Day.Current?.StartingCash ?? 0m);
    public string NetSalesDisplay => Peso.Format(ShellVm.Day.Current?.NetSales ?? 0m);
    public string ExpectedCashDisplay => Peso.Format(ShellVm.Day.Current?.ExpectedCash ?? 0m);

    public IReadOnlyList<Movement> Movements =>
        (ShellVm.Day.Current?.Movements ?? [])
            .Select(m => new Movement(m.Note, m.Amount, m.Amount < 0m, m.IsVoided))
            .ToList();

    public string ClosedTitle =>
        ShellVm.Day.LatestClosed is { } closed ? $"Shift #{closed.Number}" : "No shift yet";
    public string ClosedSub => ShellVm.Day.LatestClosed is { } x
        ? $"X read taken with {Peso.Format(x.CountedCash ?? 0m)} counted · {VarianceCaption(x.CashVariance)} · Expected {Peso.Format(x.ExpectedCash)}"
        : "No transactions yet.";

    public string OpenButtonText => $"DECLARE STARTING CASH — OPEN SHIFT #{ShellVm.Day.NextNumber}";

    public string DayTitle => ShellVm.Day.CurrentDay is { } d
        ? $"Day #{d.Number}"
        : ShellVm.Day.LatestClosedDay is { } z
            ? $"Day #{z.Number}"
            : "No day yet";
    public bool IsDayOpen => ShellVm.Day.IsDayOpen;
    public bool HasClosedDay => ShellVm.Day.HasClosedDay;
    public bool DayClosedLate => ShellVm.Day.LatestClosedDay?.ClosedLate == true;
    public string DaySub => ShellVm.Day.CurrentDay is { } d
        ? $"Opened {d.OpenedAt.ToLocalTime():h:mm tt} · {d.Shifts.Count(s => s.IsClosed)} shift(s) ended · Net sales {Peso.Format(d.NetSales)}"
        : ShellVm.Day.LatestClosedDay is { } z
            ? $"Closed with {Peso.Format(z.CountedCash ?? 0m)} counted · {VarianceCaption(z.CashVariance)} · {z.ShiftCount} shift(s)"
            : $"Opening shift #{ShellVm.Day.NextNumber} starts day #{ShellVm.Day.NextDayNumber}.";
    public bool CanCloseDay => ShellVm.Day.IsDayOpen && !ShellVm.Day.IsShiftOpen;
    public bool ShowCloseDayHint => ShellVm.Day.IsDayOpen && ShellVm.Day.IsShiftOpen;
    public bool CanOpenShift => !ShellVm.Day.IsStoreClosedToday;
    public bool IsStoreClosedToday => ShellVm.Day.IsStoreClosedToday;
    public string StoreClosedNote =>
        $"Day #{ShellVm.Day.LatestClosedDay?.Number} is closed — the register opens again after midnight.";

    private static string VarianceCaption(decimal? variance) => (variance ?? 0m) switch
    {
        < 0m => $"short by {Peso.Format(-(variance ?? 0m))}",
        > 0m => $"over by {Peso.Format(variance ?? 0m)}",
        _ => "balanced",
    };

    public string ReceiptTitle => ShellVm.Day.Current is { } live
        ? $"X READ — SHIFT #{live.Number}"
        : ShellVm.Day.IsDayOpen && ShellVm.Day.LatestClosed is { } x
            ? $"X READ #{x.Number} — SHIFT #{x.Number}"
            : ShellVm.Day.LatestClosedDay is { } z
                ? $"Z READ — DAY #{z.Number}"
                : "NO READS YET";
    public string ReceiptCaption => ShellVm.Day.Current is not null
        ? "Mid-shift · live preview"
        : ShellVm.Day.IsDayOpen && ShellVm.Day.LatestClosed is not null
            ? "Handover report · frozen"
            : ShellVm.Day.LatestClosedDay is not null
                ? "Closeout report · final"
                : "";
    public IReadOnlyList<ReceiptRow> ReceiptRows => ShellVm.Day.Current is { } live
        ? ShiftReceipt.Rows(live)
        : ShellVm.Day.IsDayOpen && ShellVm.Day.LatestClosed is { } x
            ? ShiftReceipt.Rows(x)
            : ShellVm.Day.LatestClosedDay is { } z
                ? DayReceipt.Rows(z)
                : [];

    [RelayCommand]
    private void OpenStartingCash()
    {
        if (ShellVm.Day.IsStoreClosedToday)
        {
            ShellVm.ShowToast(StoreClosedNote);
            return;
        }
        ShellVm.OpenModal(new SharedComp.StartingCashModalViewModel(ShellVm));
    }

    [RelayCommand]
    private void OpenMovement()
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            return;
        }
        ShellVm.OpenModal(new ShiftComp.MovementModalViewModel(ShellVm));
    }

    [RelayCommand]
    private void EndShift()
    {
        if (!ShellVm.Day.IsShiftOpen)
        {
            return;
        }
        ShellVm.OpenModal(new ShiftComp.XReadModalViewModel(ShellVm));
    }

    [RelayCommand]
    private void CloseDay()
    {
        if (!ShellVm.Day.IsDayOpen)
        {
            return;
        }
        if (ShellVm.Day.IsShiftOpen)
        {
            ShellVm.ShowToast("End the shift first — every drawer is counted before the day closes.");
            return;
        }
        ShellVm.OpenModal(new SharedComp.DayCloseModalViewModel(ShellVm));
    }
}
