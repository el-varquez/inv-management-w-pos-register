using POS.Register.Features.Auth.Services;
using POS.Register.Store;

namespace POS.Register.Services;

public class AppServices
{
    public SessionStore Session { get; } = new();
    public SettingsStore Settings { get; } = new();
    public ShiftStore ShiftState { get; } = new();
    public AuthService Auth { get; }
    public SettingsService StoreSettings { get; }
    public ShiftService Shifts { get; }
    public DayService Days { get; }

    public event Action? SessionExpired;

    public AppServices()
    {
        var client = new ApiClient(AppConfig.Load(), Session);
        client.SessionExpired += () => SessionExpired?.Invoke();
        Auth = new AuthService(client);
        StoreSettings = new SettingsService(client);
        Shifts = new ShiftService(client);
        Days = new DayService(client);
    }
}
