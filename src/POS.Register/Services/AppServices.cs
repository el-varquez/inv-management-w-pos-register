using POS.Register.Features.Auth.Services;
using POS.Register.Store;

namespace POS.Register.Services;

public class AppServices
{
    public SessionStore Session { get; } = new();
    public SettingsStore Settings { get; } = new();
    public AuthService Auth { get; }
    public SettingsService StoreSettings { get; }

    public event Action? SessionExpired;

    public AppServices()
    {
        var client = new ApiClient(AppConfig.Load(), Session);
        client.SessionExpired += () => SessionExpired?.Invoke();
        Auth = new AuthService(client);
        StoreSettings = new SettingsService(client);
    }
}
