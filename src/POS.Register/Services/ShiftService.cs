using POS.Register.Types;

namespace POS.Register.Services;

public class ShiftService
{
    private readonly ApiClient _api;
    public ShiftService(ApiClient api) => _api = api;

    public Task<ShiftReadDto?> GetCurrentAsync()
        => _api.GetOptionalAsync<ShiftReadDto>("shifts/current");

    public Task<PagedResult<ShiftSummaryDto>> GetShiftsAsync(int page, int pageSize)
        => _api.GetAsync<PagedResult<ShiftSummaryDto>>($"shifts?page={page}&pageSize={pageSize}");

    public Task<ShiftReadDto> GetReadAsync(Guid shiftId)
        => _api.GetAsync<ShiftReadDto>($"shifts/{shiftId}");

    public Task OpenAsync(decimal startingCash, decimal? startingEWalletBalance)
        => _api.PostAsync("shifts/open",
            new { StartingCash = startingCash, StartingEWalletBalance = startingEWalletBalance });

    public Task CloseAsync(Guid shiftId, decimal countedCash, decimal? countedEWalletBalance)
        => _api.PostAsync($"shifts/{shiftId}/close",
            new { CountedCash = countedCash, CountedEWalletBalance = countedEWalletBalance });

    public Task RecordMovementAsync(decimal amount, string note)
        => _api.PostAsync("shifts/movements", new { Amount = amount, Note = note });
}
