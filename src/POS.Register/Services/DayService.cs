using POS.Register.Types;

namespace POS.Register.Services;

public class DayService
{
    private readonly ApiClient _api;
    public DayService(ApiClient api) => _api = api;

    public Task<DayReadDto?> GetCurrentAsync()
        => _api.GetOptionalAsync<DayReadDto>("days/current");

    public Task<PagedResult<DaySummaryDto>> GetDaysAsync(int page, int pageSize)
        => _api.GetAsync<PagedResult<DaySummaryDto>>($"days?page={page}&pageSize={pageSize}");

    public Task<DayReadDto> GetReadAsync(Guid dayId)
        => _api.GetAsync<DayReadDto>($"days/{dayId}");

    public Task CloseAsync()
        => _api.PostAsync("days/close");

    public Task CloseAsync(string adminToken)
        => _api.PostAsync("days/close", null, adminToken);
}
