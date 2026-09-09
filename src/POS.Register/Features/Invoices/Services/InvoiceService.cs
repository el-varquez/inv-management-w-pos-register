using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Features.Invoices.Services;

public class InvoiceService
{
    private readonly ApiClient _api;
    public InvoiceService(ApiClient api) => _api = api;

    public Task<CreateInvoiceResult> CreateAsync(
        IList<SaleItemInput> items, decimal invoiceDiscount, Guid sukiId)
        => _api.PostAsync<CreateInvoiceResult>("invoices", new
        {
            Items = items,
            InvoiceDiscount = invoiceDiscount,
            SukiId = sukiId
        });

    public async Task<List<InvoiceDto>> GetTodayAsync()
    {
        var from = DateTime.Today.ToUniversalTime();
        var to = from.AddDays(1);
        var invoices = new List<InvoiceDto>();
        var page = 1;
        while (true)
        {
            var result = await _api.GetAsync<PagedResult<InvoiceDto>>(
                $"invoices?from={from:O}&to={to:O}&page={page}&pageSize=100");
            invoices.AddRange(result.Items);
            if (invoices.Count >= result.TotalCount || result.Items.Count == 0)
            {
                return invoices;
            }
            page++;
        }
    }

    public Task<InvoiceDetailDto> GetDetailAsync(Guid id)
        => _api.GetAsync<InvoiceDetailDto>($"invoices/{id}");

    public Task VoidAsync(Guid id, string adminToken)
        => _api.PostAsync($"invoices/{id}/void", null, adminToken);
}
