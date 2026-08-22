using POS.Register.Types;

namespace POS.Register.Services;

public class UtangService
{
    private readonly ApiClient _api;
    public UtangService(ApiClient api) => _api = api;

    public async Task<List<SukiDto>> GetSukisAsync(string? term = null)
    {
        var sukis = new List<SukiDto>();
        var page = 1;
        while (true)
        {
            var query = $"utang/sukis?page={page}&pageSize=100";
            if (!string.IsNullOrWhiteSpace(term))
            {
                query += $"&term={Uri.EscapeDataString(term)}";
            }
            var result = await _api.GetAsync<PagedResult<SukiDto>>(query);
            sukis.AddRange(result.Items);
            if (sukis.Count >= result.TotalCount || result.Items.Count == 0)
            {
                return sukis;
            }
            page++;
        }
    }

    public Task<SukiDto> CreateSukiAsync(string name, string? phone)
        => _api.PostAsync<SukiDto>("utang/sukis", new { Name = name, Phone = phone });

    public Task<SukiLedgerDto> GetLedgerAsync(Guid id)
        => _api.GetAsync<SukiLedgerDto>($"utang/sukis/{id}/ledger");

    public Task CollectAsync(Guid sukiId, decimal amount)
        => _api.PostAsync("utang/collect", new { SukiId = sukiId, Amount = amount });

    public Task VoidPaymentAsync(Guid id, string adminToken)
        => _api.PostAsync($"utang/payments/{id}/void", null, adminToken);

    public Task EditPaymentAsync(Guid id, decimal amount, string adminToken)
        => _api.PutAsync($"utang/payments/{id}", new { Amount = amount }, adminToken);

    public Task VoidChargeAsync(Guid transactionId, string adminToken)
        => _api.PostAsync($"sales/{transactionId}/refund", null, adminToken);
}
