using POS.Register.Features.Auth.Services;
using POS.Register.Features.Invoices.Services;
using POS.Register.Features.Sell.Services;
using POS.Register.Store;

namespace POS.Register.Services;

public class AppServices
{
    public SessionStore Session { get; } = new();
    public SettingsStore Settings { get; } = new();
    public ShiftStore ShiftState { get; } = new();
    public CartStore Cart { get; } = new();
    public CartStore InvoiceCart { get; } = new();
    public PaymentMethodStore MethodStore { get; } = new();
    public AuthService Auth { get; }
    public SettingsService StoreSettings { get; }
    public PaymentMethodService PaymentMethods { get; }
    public CatalogService Catalog { get; }
    public ShiftService Shifts { get; }
    public DayService Days { get; }
    public SellService Sell { get; }
    public SalesService Sales { get; }
    public InvoiceService Invoices { get; }
    public UtangService Utang { get; }

    public event Action? SessionExpired;

    public AppServices()
    {
        var client = new ApiClient(AppConfig.Load(), Session);
        client.SessionExpired += () => SessionExpired?.Invoke();
        Auth = new AuthService(client);
        StoreSettings = new SettingsService(client);
        PaymentMethods = new PaymentMethodService(client);
        Catalog = new CatalogService(client);
        Shifts = new ShiftService(client);
        Days = new DayService(client);
        Sell = new SellService(client);
        Sales = new SalesService(client);
        Invoices = new InvoiceService(client);
        Utang = new UtangService(client);
    }
}
