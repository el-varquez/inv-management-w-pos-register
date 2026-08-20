using CommunityToolkit.Mvvm.ComponentModel;
using POS.Register.Types;

namespace POS.Register.Store;

public partial class ShiftStore : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShiftOpen), nameof(HasClosedShift), nameof(ChipText), nameof(NextNumber), nameof(IsStoreClosedToday))]
    private ShiftReadDto? current;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasClosedShift), nameof(ChipText), nameof(NextNumber))]
    private ShiftReadDto? latestClosed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDayOpen), nameof(HasClosedDay), nameof(ChipText), nameof(NextDayNumber), nameof(IsStoreClosedToday))]
    private DayReadDto? currentDay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasClosedDay), nameof(ChipText), nameof(NextDayNumber), nameof(IsStoreClosedToday))]
    private DayReadDto? latestClosedDay;

    public bool IsShiftOpen => Current is not null;

    public bool IsDayOpen => CurrentDay is not null;

    public bool HasClosedShift => Current is null && LatestClosed is not null;

    public bool HasClosedDay => CurrentDay is null && LatestClosedDay is not null;

    public bool IsStoreClosedToday =>
        Current is null
        && CurrentDay is null
        && LatestClosedDay is { } day
        && day.OpenedAt.ToLocalTime().Date == DateTime.Now.Date;

    public int NextNumber => (Current?.Number ?? LatestClosed?.Number ?? 0) + (Current is null ? 1 : 0);

    public int NextDayNumber => CurrentDay?.Number ?? (LatestClosedDay?.Number ?? 0) + 1;

    public string ChipText => Current is not null
        ? $"Day #{NextDayNumber} · Shift #{Current.Number} · Open {Current.OpenedAt.ToLocalTime():h:mm tt}"
        : CurrentDay is not null
            ? $"Day #{CurrentDay.Number} · NO SHIFT"
            : LatestClosedDay is not null
                ? $"CLOSED · DAY #{LatestClosedDay.Number}"
                : "NO DAY YET";
}
