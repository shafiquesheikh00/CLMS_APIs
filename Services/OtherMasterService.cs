using CLMS_APIs.Data;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services;

public class OtherMasterService : IOtherMasterService
{
    private readonly ClmsDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<OtherMasterService> _logger;

    public OtherMasterService(
        ClmsDbContext context,
        IAuditLogService auditLogService,
        ILogger<OtherMasterService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<MasterCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OtherMasters
            .AsNoTracking()
            .GroupBy(x => new { x.MasterId, x.MasterName })
            .Select(g => new MasterCategoryDto
            {
                MasterId = g.Key.MasterId,
                MasterName = g.Key.MasterName
            })
            .OrderBy(c => c.MasterName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OtherMasterListDto>> GetAllAsync(int? masterId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<OtherMaster> query = _context.OtherMasters.AsNoTracking();

        if (masterId.HasValue)
        {
            query = query.Where(x => x.MasterId == masterId.Value);
        }

        return await query
            .OrderBy(x => x.MasterTypeId)
            .Select(x => new OtherMasterListDto
            {
                MasterTypeId = x.MasterTypeId,
                MasterId = x.MasterId,
                MasterName = x.MasterName,
                MasterType = x.MasterType,
                Description = x.Description,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OtherMasterLookupDto>> GetLookupByMasterNameAsync(string masterName, CancellationToken cancellationToken = default)
    {
        var trimmedName = masterName.Trim().ToLower();

        return await _context.OtherMasters
            .AsNoTracking()
            .Where(x => x.MasterName.ToLower() == trimmedName && (x.Status == null || x.Status == true))
            .OrderBy(x => x.MasterType)
            .Select(x => new OtherMasterLookupDto
            {
                MasterTypeId = x.MasterTypeId,
                MasterType = x.MasterType
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OtherMasterListDto?> GetByIdAsync(int masterTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.OtherMasters
            .AsNoTracking()
            .Where(x => x.MasterTypeId == masterTypeId)
            .Select(x => new OtherMasterListDto
            {
                MasterTypeId = x.MasterTypeId,
                MasterId = x.MasterId,
                MasterName = x.MasterName,
                MasterType = x.MasterType,
                Description = x.Description,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OtherMasterListDto> CreateAsync(OtherMasterCreateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        int masterId;
        string masterName;
        var trimmedMasterType = dto.MasterType.Trim();

        if (dto.MasterId.HasValue && dto.MasterId.Value > 0)
        {
            // Path 1: Existing Category selected
            var existingCategory = await _context.OtherMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MasterId == dto.MasterId.Value, cancellationToken);

            if (existingCategory == null)
            {
                throw new ArgumentException($"Category with MasterID '{dto.MasterId.Value}' was not found.");
            }

            masterId = existingCategory.MasterId;
            masterName = existingCategory.MasterName;

            // Duplicate MasterType check within this category
            var exists = await _context.OtherMasters
                .AnyAsync(x => x.MasterId == masterId && x.MasterType.ToLower() == trimmedMasterType.ToLower(), cancellationToken);

            if (exists)
            {
                throw new DuplicateEntityException($"MasterType '{trimmedMasterType}' already exists under category '{masterName}'.");
            }
        }
        else
        {
            // Path 2: New Category being created
            var newCategoryName = (dto.NewMasterName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newCategoryName))
            {
                throw new ArgumentException("NewMasterName is required when creating a new category.");
            }

            // Check if category name already exists
            var existingCategory = await _context.OtherMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MasterName.ToLower() == newCategoryName.ToLower(), cancellationToken);

            if (existingCategory != null)
            {
                masterId = existingCategory.MasterId;
                masterName = existingCategory.MasterName;

                var exists = await _context.OtherMasters
                    .AnyAsync(x => x.MasterId == masterId && x.MasterType.ToLower() == trimmedMasterType.ToLower(), cancellationToken);

                if (exists)
                {
                    throw new DuplicateEntityException($"MasterType '{trimmedMasterType}' already exists under category '{masterName}'.");
                }
            }
            else
            {
                var maxMasterId = await _context.OtherMasters
                    .Select(x => (int?)x.MasterId)
                    .MaxAsync(cancellationToken) ?? 0;

                masterId = maxMasterId + 1;
                masterName = newCategoryName;
            }
        }

        var entity = new OtherMaster
        {
            MasterId = masterId,
            MasterName = masterName,
            MasterType = trimmedMasterType,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Status = true
        };

        _context.OtherMasters.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Other Master", "CREATE", entity.MasterTypeId.ToString(), userId, cancellationToken);

        return new OtherMasterListDto
        {
            MasterTypeId = entity.MasterTypeId,
            MasterId = entity.MasterId,
            MasterName = entity.MasterName,
            MasterType = entity.MasterType,
            Description = entity.Description,
            Status = entity.Status
        };
    }

    public async Task<OtherMasterListDto?> UpdateAsync(int masterTypeId, OtherMasterUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OtherMasters
            .FirstOrDefaultAsync(x => x.MasterTypeId == masterTypeId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        var trimmedMasterType = dto.MasterType.Trim();

        // Duplicate check within same category (excluding current record)
        var exists = await _context.OtherMasters
            .AnyAsync(x => x.MasterTypeId != masterTypeId &&
                           x.MasterId == entity.MasterId &&
                           x.MasterType.ToLower() == trimmedMasterType.ToLower(), cancellationToken);

        if (exists)
        {
            throw new DuplicateEntityException($"MasterType '{trimmedMasterType}' already exists under category '{entity.MasterName}'.");
        }

        entity.MasterType = trimmedMasterType;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Other Master", "UPDATE", masterTypeId.ToString(), userId, cancellationToken);

        return new OtherMasterListDto
        {
            MasterTypeId = entity.MasterTypeId,
            MasterId = entity.MasterId,
            MasterName = entity.MasterName,
            MasterType = entity.MasterType,
            Description = entity.Description,
            Status = entity.Status
        };
    }

    public async Task<bool> DeleteAsync(int masterTypeId, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OtherMasters
            .FirstOrDefaultAsync(x => x.MasterTypeId == masterTypeId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        _context.OtherMasters.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Other Master", "DELETE", masterTypeId.ToString(), userId, cancellationToken);

        return true;
    }
}
