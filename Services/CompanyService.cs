using CLMS_APIs.Data;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services;

public class CompanyService : ICompanyService
{
    private readonly ClmsDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ClmsDbContext context,
        IWebHostEnvironment environment,
        IAuditLogService auditLogService,
        ILogger<CompanyService> logger)
    {
        _context = context;
        _environment = environment;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<CompanyDto>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .Select(c => new CompanyDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName,
                CompanyAddress = c.CompanyAddress,
                Email = c.EmailId,
                Phone = c.Phone,
                AlternateTelNo = c.AlternateTelNo,
                LogoUrl = c.CompLogo
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyDetailDto?> GetCompanyByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);

        if (company == null)
        {
            return null;
        }

        return MapToDetailDto(company);
    }

    public async Task<CompanyDetailDto> CreateCompanyAsync(CompanyCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var trimmedName = dto.CompanyName.Trim();
        var isDuplicate = await _context.Companies
            .AnyAsync(c => c.CompanyName.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new DuplicateEntityException($"A company with the name '{dto.CompanyName}' already exists.");
        }

        var nextId = await GenerateNextCompanyIdAsync(cancellationToken);

        string? logoRelativeUrl = null;
        if (dto.Logo != null && dto.Logo.Length > 0)
        {
            logoRelativeUrl = await SaveLogoFileAsync(nextId, dto.Logo, cancellationToken);
        }

        var company = new CompanyMaster
        {
            CompanyId = nextId,
            CompanyName = trimmedName,
            CompanyAddress = dto.CompanyAddress.Trim(),
            CompLogo = logoRelativeUrl,
            EntLog = DateTime.Now,
            Tin = dto.Tin?.Trim(),
            Pan = dto.Pan?.Trim(),
            Phone = dto.Phone?.Trim(),
            Fax = dto.Fax?.Trim(),
            EmailId = dto.Email?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            Website = dto.Website?.Trim(),
            GressTime = dto.GressTime,
            RegFingerCount = dto.RegFingerCount,
            AlternateTelNo = dto.AlternateTelNo
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("CompanyMaster", "CREATE", company.CompanyId, userId, cancellationToken);

        return MapToDetailDto(company);
    }

    public async Task<CompanyDetailDto?> UpdateCompanyAsync(string id, CompanyCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);

        if (company == null)
        {
            return null;
        }

        var trimmedName = dto.CompanyName.Trim();
        var isDuplicate = await _context.Companies
            .AnyAsync(c => c.CompanyId != id && c.CompanyName.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new DuplicateEntityException($"A company with the name '{dto.CompanyName}' already exists.");
        }

        if (dto.Logo != null && dto.Logo.Length > 0)
        {
            // Delete existing logo file if present
            DeleteLogoFile(company.CompLogo);

            // Save new logo file
            company.CompLogo = await SaveLogoFileAsync(id, dto.Logo, cancellationToken);
        }

        company.CompanyName = trimmedName;
        company.CompanyAddress = dto.CompanyAddress.Trim();
        company.Tin = dto.Tin?.Trim();
        company.Pan = dto.Pan?.Trim();
        company.Phone = dto.Phone?.Trim();
        company.Fax = dto.Fax?.Trim();
        company.EmailId = dto.Email?.Trim();
        company.ContactPerson = dto.ContactPerson?.Trim();
        company.Website = dto.Website?.Trim();
        company.GressTime = dto.GressTime;
        company.RegFingerCount = dto.RegFingerCount;
        company.AlternateTelNo = dto.AlternateTelNo;
        company.ModDt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("CompanyMaster", "UPDATE", id, userId, cancellationToken);

        return MapToDetailDto(company);
    }

    public async Task<bool> DeleteCompanyAsync(string id, string? userId, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);

        if (company == null)
        {
            return false;
        }

        // Delete company logo directory on disk
        DeleteCompanyDirectory(id);

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("CompanyMaster", "DELETE", id, userId, cancellationToken);

        return true;
    }

    private async Task<string> GenerateNextCompanyIdAsync(CancellationToken cancellationToken)
    {
        var existingIds = await _context.Companies
            .Select(c => c.CompanyId)
            .ToListAsync(cancellationToken);

        int maxNumericId = 0;
        foreach (var id in existingIds)
        {
            if (int.TryParse(id, out var num))
            {
                if (num > maxNumericId) maxNumericId = num;
            }
            else
            {
                var digits = new string(id.Where(char.IsDigit).ToArray());
                if (int.TryParse(digits, out var prefixNum) && prefixNum > maxNumericId)
                {
                    maxNumericId = prefixNum;
                }
            }
        }

        return (maxNumericId + 1).ToString();
    }

    private async Task<string> SaveLogoFileAsync(string companyId, IFormFile file, CancellationToken cancellationToken)
    {
        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "company-logos", companyId);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsFolder, safeFileName);

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"/uploads/company-logos/{companyId}/{safeFileName}";
    }

    private void DeleteLogoFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return;

        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var cleanRelative = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRoot, cleanRelative);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete old logo file: {Url}", relativeUrl);
        }
    }

    private void DeleteCompanyDirectory(string companyId)
    {
        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var companyFolder = Path.Combine(webRoot, "uploads", "company-logos", companyId);

            if (Directory.Exists(companyFolder))
            {
                Directory.Delete(companyFolder, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete company directory for CompanyId: {CompanyId}", companyId);
        }
    }

    private static CompanyDetailDto MapToDetailDto(CompanyMaster company)
    {
        return new CompanyDetailDto
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            CompanyAddress = company.CompanyAddress,
            LogoUrl = company.CompLogo,
            EntLog = company.EntLog,
            ModDt = company.ModDt,
            Tin = company.Tin,
            Pan = company.Pan,
            Phone = company.Phone,
            Fax = company.Fax,
            Email = company.EmailId,
            ContactPerson = company.ContactPerson,
            Website = company.Website,
            GressTime = company.GressTime,
            RegFingerCount = company.RegFingerCount,
            AlternateTelNo = company.AlternateTelNo
        };
    }
}
