using CommunityToolkit.Mvvm.ComponentModel;

namespace POS.Register.Shell;

public partial class DayState : ObservableObject
{
    [ObservableProperty]
    private bool isShiftOpen;

    [ObservableProperty]
    private bool isClosed;

    [ObservableProperty]
    private bool closedLate;
}
