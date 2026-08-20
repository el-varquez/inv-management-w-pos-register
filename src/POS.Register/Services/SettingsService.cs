using POS.Register.Types;

namespace POS.Register.Services;

public class SettingsService
{
    private readonly ApiClient _api;
    public SettingsService(ApiClient api) => _api = api;

    public Task<StoreNameResponse> GetStoreNameAsync()
        => _api.GetAsync<StoreNameResponse>("settings/store-name");

    public Task<StoreSettingsDto> GetSettingsAsync()
        => _api.GetAsync<StoreSettingsDto>("settings");
}
