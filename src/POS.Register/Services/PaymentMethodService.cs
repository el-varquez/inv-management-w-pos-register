using POS.Register.Types;

namespace POS.Register.Services;

public class PaymentMethodService
{
    private readonly ApiClient _api;
    public PaymentMethodService(ApiClient api) => _api = api;

    public Task<List<PaymentMethodDto>> GetAllAsync()
        => _api.GetAsync<List<PaymentMethodDto>>("payment-methods");
}
