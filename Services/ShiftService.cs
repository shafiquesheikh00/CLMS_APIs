using CLMS_APIs.Data;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services;

public class ShiftService : IShiftService
{
    private readonly ClmsDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(
        ClmsDbContext context,
        IAuditLogService auditLogService,
        ILogger<ShiftService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<ShiftListDto>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shifts
            .AsNoTracking()
            .OrderBy(s => s.ShiftId)
            .Select(s => new ShiftListDto
            {
                ShiftId = s.ShiftId,
                ShiftName = s.ShiftName,
                StartTime = s.Start_Time.ToString("HH:mm"),
                EndTime = s.End_Time.ToString("HH:mm"),
                ShiftHours = s.ShiftHours ?? 0,
                IsOvernight = s.Shift_Flag ?? false,
                GraceTime = s.GressTime ?? 0
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ShiftListDto?> GetShiftByIdAsync(decimal shiftId, CancellationToken cancellationToken = default)
    {
        return await _context.Shifts
            .AsNoTracking()
            .Where(s => s.ShiftId == shiftId)
            .Select(s => new ShiftListDto
            {
                ShiftId = s.ShiftId,
                ShiftName = s.ShiftName,
                StartTime = s.Start_Time.ToString("HH:mm"),
                EndTime = s.End_Time.ToString("HH:mm"),
                ShiftHours = s.ShiftHours ?? 0,
                IsOvernight = s.Shift_Flag ?? false,
                GraceTime = s.GressTime ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ShiftListDto> CreateShiftAsync(ShiftCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var trimmedName = dto.ShiftName.Trim();

        // 1. Unique ShiftName check (case-insensitive)
        var duplicate = await _context.Shifts
            .AnyAsync(s => s.ShiftName.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (duplicate)
        {
            throw new DuplicateEntityException($"Shift with name '{trimmedName}' already exists.");
        }

        // 2. Validate timing & calculate ShiftHours server-side
        var timing = CalculateShiftTiming(dto.StartTime, dto.EndTime, dto.IsOvernight);

        // 3. Generate ShiftID via max + 1
        var maxId = await _context.Shifts
            .Select(s => (decimal?)s.ShiftId)
            .MaxAsync(cancellationToken) ?? 0;

        var nextId = maxId + 1;

        var entity = new ShiftMaster
        {
            ShiftId = nextId,
            ShiftName = trimmedName,
            Start_Time = timing.StartDateTime,
            End_Time = timing.EndDateTime,
            Shift_Flag = dto.IsOvernight,
            ShiftHours = timing.ShiftHours,
            GressTime = dto.GraceTime
        };

        _context.Shifts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Audit Log
        await _auditLogService.LogAsync("Shift Master", "CREATE", entity.ShiftId.ToString(), userId, cancellationToken);

        return new ShiftListDto
        {
            ShiftId = entity.ShiftId,
            ShiftName = entity.ShiftName,
            StartTime = entity.Start_Time.ToString("HH:mm"),
            EndTime = entity.End_Time.ToString("HH:mm"),
            ShiftHours = entity.ShiftHours ?? 0,
            IsOvernight = entity.Shift_Flag ?? false,
            GraceTime = entity.GressTime ?? 0
        };
    }

    public async Task<ShiftListDto?> UpdateShiftAsync(decimal shiftId, ShiftCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Shifts
            .FirstOrDefaultAsync(s => s.ShiftId == shiftId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        var trimmedName = dto.ShiftName.Trim();

        // Unique ShiftName check against other records
        var duplicate = await _context.Shifts
            .AnyAsync(s => s.ShiftId != shiftId && s.ShiftName.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (duplicate)
        {
            throw new DuplicateEntityException($"Shift with name '{trimmedName}' already exists.");
        }

        // Validate timing & calculate ShiftHours server-side
        var timing = CalculateShiftTiming(dto.StartTime, dto.EndTime, dto.IsOvernight);

        entity.ShiftName = trimmedName;
        entity.Start_Time = timing.StartDateTime;
        entity.End_Time = timing.EndDateTime;
        entity.Shift_Flag = dto.IsOvernight;
        entity.ShiftHours = timing.ShiftHours;
        entity.GressTime = dto.GraceTime;

        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Shift Master", "UPDATE", shiftId.ToString(), userId, cancellationToken);

        return new ShiftListDto
        {
            ShiftId = entity.ShiftId,
            ShiftName = entity.ShiftName,
            StartTime = entity.Start_Time.ToString("HH:mm"),
            EndTime = entity.End_Time.ToString("HH:mm"),
            ShiftHours = entity.ShiftHours ?? 0,
            IsOvernight = entity.Shift_Flag ?? false,
            GraceTime = entity.GressTime ?? 0
        };
    }

    public async Task<bool> DeleteShiftAsync(decimal shiftId, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Shifts
            .FirstOrDefaultAsync(s => s.ShiftId == shiftId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        // TODO: In the future, check if any EmployeeMaster / Attendance records reference this ShiftId before deleting.
        _context.Shifts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Shift Master", "DELETE", shiftId.ToString(), userId, cancellationToken);

        return true;
    }

    /// <summary>
    /// Validates shift start/end times and calculates ShiftHours accurately for both same-day and overnight shifts.
    /// </summary>
    public (decimal ShiftHours, DateTime StartDateTime, DateTime EndDateTime) CalculateShiftTiming(
        string startTimeStr, 
        string endTimeStr, 
        bool isOvernight)
    {
        var start = ParseTime(startTimeStr, "start time");
        var end = ParseTime(endTimeStr, "end time");

        // Same-day shift validation
        if (!isOvernight && end <= start)
        {
            throw new ArgumentException("End time must be after start time for a same-day shift.");
        }

        DateTime baseDate = new DateTime(1900, 1, 1);
        DateTime startDt = baseDate.Add(start.ToTimeSpan());
        DateTime endDt;
        double totalHours;

        if (isOvernight)
        {
            if (end <= start)
            {
                // Overnight shift crossing midnight to next day (e.g. 22:00 -> 06:00 = 8 hrs)
                endDt = baseDate.AddDays(1).Add(end.ToTimeSpan());
                totalHours = (endDt - startDt).TotalHours;
            }
            else
            {
                // Overnight shift within same day range
                endDt = baseDate.Add(end.ToTimeSpan());
                totalHours = (endDt - startDt).TotalHours;
            }
        }
        else
        {
            endDt = baseDate.Add(end.ToTimeSpan());
            totalHours = (endDt - startDt).TotalHours;
        }

        if (totalHours <= 0 || totalHours > 24)
        {
            throw new ArgumentException($"Calculated shift duration of {totalHours:F2} hours is invalid. Shift duration must be between 0 and 24 hours.");
        }

        decimal shiftHours = Math.Round((decimal)totalHours, 2);
        return (shiftHours, startDt, endDt);
    }

    private static TimeOnly ParseTime(string timeStr, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(timeStr))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }

        if (TimeOnly.TryParse(timeStr, out var time))
            return time;

        if (DateTime.TryParse(timeStr, out var dt))
            return TimeOnly.FromDateTime(dt);

        if (TimeSpan.TryParse(timeStr, out var ts))
            return TimeOnly.FromTimeSpan(ts);

        throw new ArgumentException($"Invalid format for {fieldName}: '{timeStr}'. Please provide time in HH:mm format (e.g., '09:00', '22:00').");
    }
}
