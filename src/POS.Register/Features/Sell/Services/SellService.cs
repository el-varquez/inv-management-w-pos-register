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
        Guid paymentMethodId,
        decimal amountTendered,
        string? referenceNumber,
        Guid? sukiId = null,
        decimal downPayment = 0m)
        => _api.PostAsync<CreateSaleResult>("sales", new
        {
            Items = items,
            TransactionDiscount = transactionDiscount,
            PaymentMethodId = paymentMethodId,
            AmountTendered = amountTendered,
            ReferenceNumber = referenceNumber,
            SukiId = sukiId,
            DownPayment = downPayment
        });
}
