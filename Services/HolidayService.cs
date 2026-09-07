using CLMS_APIs.Data;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services;

public class HolidayService : IHolidayService
{
    private readonly ClmsDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<HolidayService> _logger;

    public HolidayService(
        ClmsDbContext context,
        IAuditLogService auditLogService,
        ILogger<HolidayService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<HolidayDto>> GetAllHolidaysAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        IQueryable<HolidayMaster> query = _context.Holidays.AsNoTracking();

        if (year.HasValue)
        {
            query = query.Where(h => h.HolidayDate.Year == year.Value);
        }

        return await query
            .OrderBy(h => h.HolidayDate)
            .Select(h => new HolidayDto
            {
                Id = h.Id,
                HolidayDate = h.HolidayDate,
                HolidayDesc = h.Holiday_Desc,
                IsPaid = h.Ispaid
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<HolidayDto?> GetHolidayByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Holidays
            .AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new HolidayDto
            {
                Id = h.Id,
                HolidayDate = h.HolidayDate,
                HolidayDesc = h.Holiday_Desc,
                IsPaid = h.Ispaid
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<HolidayDto> CreateHolidayAsync(HolidayCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        // 1. Validation: HolidayDate cannot be in the past when creating
        if (dto.HolidayDate.Date < DateTime.Today)
        {
            throw new ArgumentException("Holiday date cannot be in the past for newly created holidays.");
        }

        // 2. Duplicate Check: reject if holiday already exists on the same date
        var targetDate = dto.HolidayDate.Date;
        var dateExists = await _context.Holidays
            .AnyAsync(h => h.HolidayDate.Date == targetDate, cancellationToken);

        if (dateExists)
        {
            throw new DuplicateEntityException($"A holiday is already scheduled on {targetDate:yyyy-MM-dd}.");
        }

        var entity = new HolidayMaster
        {
            HolidayDate = dto.HolidayDate,
            Holiday_Desc = dto.HolidayDesc.Trim(),
            Ispaid = dto.IsPaid
        };

        _context.Holidays.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Audit log
        await _auditLogService.LogAsync("Holiday Master", "CREATE", entity.Id.ToString(), userId, cancellationToken);

        return new HolidayDto
        {
            Id = entity.Id,
            HolidayDate = entity.HolidayDate,
            HolidayDesc = entity.Holiday_Desc,
            IsPaid = entity.Ispaid
        };
    }

    public async Task<HolidayDto?> UpdateHolidayAsync(int id, HolidayCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Holidays.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        // Duplicate check on date for other holidays
        var targetDate = dto.HolidayDate.Date;
        var dateExists = await _context.Holidays
            .AnyAsync(h => h.Id != id && h.HolidayDate.Date == targetDate, cancellationToken);

        if (dateExists)
        {
            throw new DuplicateEntityException($"Another holiday is already scheduled on {targetDate:yyyy-MM-dd}.");
        }

        entity.HolidayDate = dto.HolidayDate;
        entity.Holiday_Desc = dto.HolidayDesc.Trim();
        entity.Ispaid = dto.IsPaid;

        await _context.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditLogService.LogAsync("Holiday Master", "UPDATE", entity.Id.ToString(), userId, cancellationToken);

        return new HolidayDto
        {
            Id = entity.Id,
            HolidayDate = entity.HolidayDate,
            HolidayDesc = entity.Holiday_Desc,
            IsPaid = entity.Ispaid
        };
    }

    public async Task<bool> DeleteHolidayAsync(int id, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Holidays.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Holidays.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditLogService.LogAsync("Holiday Master", "DELETE", id.ToString(), userId, cancellationToken);

        return true;
    }
}
