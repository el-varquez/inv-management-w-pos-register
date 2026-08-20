using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Features.Sales.Services;

public class SalesService
{
    private readonly ApiClient _api;
    public SalesService(ApiClient api) => _api = api;

    public async Task<List<SaleDto>> GetTodayAsync()
    {
        var from = DateTime.Today.ToUniversalTime();
        var to = from.AddDays(1);
        var sales = new List<SaleDto>();
        var page = 1;
        while (true)
        {
            var result = await _api.GetAsync<PagedResult<SaleDto>>(
                $"sales?from={from:O}&to={to:O}&page={page}&pageSize=100");
            sales.AddRange(result.Items);
            if (sales.Count >= result.TotalCount || result.Items.Count == 0)
            {
                return sales;
            }
            page++;
        }
    }

    public Task<SaleDetailDto> GetDetailAsync(Guid id)
        => _api.GetAsync<SaleDetailDto>($"sales/{id}");

    public Task RefundAsync(Guid id, string adminToken)
        => _api.PostAsync($"sales/{id}/refund", null, adminToken);
}
