using CLMS_APIs.Models.DTOs;

namespace CLMS_APIs.Services;

public interface IShiftService
{
    Task<List<ShiftListDto>> GetAllShiftsAsync(CancellationToken cancellationToken = default);
    Task<ShiftListDto?> GetShiftByIdAsync(decimal shiftId, CancellationToken cancellationToken = default);
    Task<ShiftListDto> CreateShiftAsync(ShiftCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<ShiftListDto?> UpdateShiftAsync(decimal shiftId, ShiftCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteShiftAsync(decimal shiftId, string? userId, CancellationToken cancellationToken = default);
    (decimal ShiftHours, DateTime StartDateTime, DateTime EndDateTime) CalculateShiftTiming(string startTimeStr, string endTimeStr, bool isOvernight);
}
