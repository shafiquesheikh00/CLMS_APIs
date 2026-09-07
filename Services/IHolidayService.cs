using CLMS_APIs.Models.DTOs;

namespace CLMS_APIs.Services;

public interface IHolidayService
{
    Task<List<HolidayDto>> GetAllHolidaysAsync(int? year = null, CancellationToken cancellationToken = default);
    Task<HolidayDto?> GetHolidayByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<HolidayDto> CreateHolidayAsync(HolidayCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<HolidayDto?> UpdateHolidayAsync(int id, HolidayCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteHolidayAsync(int id, string? userId, CancellationToken cancellationToken = default);
}
