using CommunityToolkit.Mvvm.ComponentModel;
using POS.Register.Lib;

namespace POS.Register.Store;

public partial class SettingsStore : ObservableObject
{
    [ObservableProperty]
    private string storeName = CannedDay.StoreName;

    [ObservableProperty]
    private decimal defaultUtangMarkup;

    [ObservableProperty]
    private bool trackEWalletFloat;
}
