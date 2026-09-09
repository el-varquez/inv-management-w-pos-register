using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Features.Sell.Services;

public class SellService
{
    private readonly ApiClient _api;
    public SellService(ApiClient api) => _api = api;

    public Task<CreateSaleResult> CompleteSaleAsync(
        IList<SaleItemInput> items,
        decimal transactionDiscount,
        Guid paymentMethodId,
        decimal amountTendered,
        string? referenceNumber)
        => _api.PostAsync<CreateSaleResult>("sales", new
        {
            Items = items,
            TransactionDiscount = transactionDiscount,
            PaymentMethodId = paymentMethodId,
            AmountTendered = amountTendered,
            ReferenceNumber = referenceNumber
        });
}
