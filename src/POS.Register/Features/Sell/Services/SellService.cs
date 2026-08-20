using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Features.Sell.Services;

public class SellService
{
    private readonly ApiClient _api;
    public SellService(ApiClient api) => _api = api;

    public Task<List<SellableItemDto>> SearchAsync(string term)
        => _api.GetAsync<List<SellableItemDto>>(
            $"items/search-sellable?term={Uri.EscapeDataString(term)}");

    public Task<List<PopularItemDto>> GetPopularAsync()
        => _api.GetAsync<List<PopularItemDto>>("items/popular");

    public Task<CreateSaleResult> CompleteSaleAsync(
        IList<SaleItemInput> items,
        decimal transactionDiscount,
        string paymentType,
        decimal amountTendered,
        string? referenceNumber)
        => _api.PostAsync<CreateSaleResult>("sales", new
        {
            Items = items,
            TransactionDiscount = transactionDiscount,
            PaymentType = paymentType,
            AmountTendered = amountTendered,
            ReferenceNumber = referenceNumber
        });
}
