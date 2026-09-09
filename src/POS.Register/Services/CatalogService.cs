using POS.Register.Types;

namespace POS.Register.Services;

public class CatalogService
{
    private readonly ApiClient _api;
    public CatalogService(ApiClient api) => _api = api;

    public Task<List<SellableItemDto>> SearchAsync(string term)
        => _api.GetAsync<List<SellableItemDto>>(
            $"items/search-sellable?term={Uri.EscapeDataString(term)}");

    public Task<List<PopularItemDto>> GetPopularAsync()
        => _api.GetAsync<List<PopularItemDto>>("items/popular");
}
