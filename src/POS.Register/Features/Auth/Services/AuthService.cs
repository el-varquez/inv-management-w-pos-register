using POS.Register.Services;
using POS.Register.Types;

namespace POS.Register.Features.Auth.Services;

public class AuthService
{
    private readonly ApiClient _api;
    public AuthService(ApiClient api) => _api = api;

    public Task<LoginResponse> LoginAsync(string username, string password)
        => _api.PostAsync<LoginResponse>("auth/login", new { Username = username, Password = password });

    public Task<LoginResponse> SetupPasswordAsync(string username, string newPassword)
        => _api.PostAsync<LoginResponse>("auth/setup-password", new { Username = username, NewPassword = newPassword });
}
