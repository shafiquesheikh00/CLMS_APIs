using CLMS_APIs.Data;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services;

public class ContractorService : IContractorService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly ClmsDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ContractorService> _logger;

    public ContractorService(
        ClmsDbContext context,
        IWebHostEnvironment environment,
        IAuditLogService auditLogService,
        ILogger<ContractorService> logger)
    {
        _context = context;
        _environment = environment;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<ContractorListDto>> GetContractorsAsync(bool? activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _context.Contractors.AsNoTracking();

        if (activeOnly.HasValue)
        {
            var today = DateTime.Today;
            if (activeOnly.Value)
            {
                query = query.Where(c => c.ValidDt != null && c.ValidDt.Value.Date >= today);
            }
            else
            {
                query = query.Where(c => c.ValidDt == null || c.ValidDt.Value.Date < today);
            }
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new ContractorListDto
            {
                ContractorId = c.ContractorId,
                Name = c.Name,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
                AdharNo = c.AdharNo,
                StdLStrength = c.StdLStrength,
                ValidDt = c.ValidDt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ContractorDetailDto?> GetContractorByIdAsync(decimal id, CancellationToken cancellationToken = default)
    {
        var contractor = await _context.Contractors
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContractorId == id, cancellationToken);

        if (contractor == null)
        {
            return null;
        }

        string? typeName = null;
        if (!string.IsNullOrWhiteSpace(contractor.ContactorType))
        {
            var other = await _context.OtherMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.MasterName == "Contractor Type" &&
                                          (o.MasterTypeId.ToString() == contractor.ContactorType || o.MasterType == contractor.ContactorType),
                                     cancellationToken);
            typeName = other?.MasterType ?? other?.Description;
        }

        return MapToDetailDto(contractor, typeName);
    }

    public async Task<List<ContractorTypeLookupDto>> GetContractorTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OtherMasters
            .AsNoTracking()
            .Where(o => o.MasterName == "Contractor Type" && (o.Status == null || o.Status == true))
            .OrderBy(o => o.MasterType ?? o.Description)
            .Select(o => new ContractorTypeLookupDto
            {
                Id = (o.MasterTypeId ?? o.MasterId).ToString(),
                Name = o.MasterType ?? o.Description ?? ("Type " + o.MasterId)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ContractorDetailDto> CreateContractorAsync(ContractorCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        // 1. Detailed duplicate validation
        await ValidateDuplicatesAsync(dto, null, cancellationToken);

        // 2. Compute next ContractorID via max + 1
        var maxId = await _context.Contractors
            .Select(c => (decimal?)c.ContractorId)
            .MaxAsync(cancellationToken) ?? 0;
        decimal nextId = maxId + 1;

        // 3. Document uploads
        string? licensePath = null;
        if (dto.LicenseDocument != null && dto.LicenseDocument.Length > 0)
        {
            licensePath = await SaveDocumentAsync(nextId, "license", dto.LicenseDocument, cancellationToken);
        }

        string? agreementPath = null;
        if (dto.AgreementDocument != null && dto.AgreementDocument.Length > 0)
        {
            agreementPath = await SaveDocumentAsync(nextId, "agreement", dto.AgreementDocument, cancellationToken);
        }

        var contractor = new ContractorMaster
        {
            ContractorId = nextId,
            Name = dto.Name,
            Address = dto.Address?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            Description = dto.Description?.Trim(),
            LogDt = DateTime.Now,
            EsiPay = dto.EsiPay,
            PfPay = dto.PfPay,
            SalPayFlag = dto.SalPayFlag,
            ContPerNm = dto.ContPerNm?.Trim(),
            ContPerPhone = dto.ContPerPhone?.Trim(),
            ContPerAddress = dto.ContPerAddress?.Trim(),
            StdLStrength = dto.StdLStrength,
            ValidDt = dto.ValidDt,
            LicNo = dto.LicNo,
            DocList = dto.DocList?.Trim(),
            FaxNo = dto.FaxNo?.Trim(),
            EsicNo = dto.EsicNo?.Trim(),
            AdharNo = dto.AdharNo,
            PanNo = dto.PanNo,
            LicenseUpload = licensePath,
            AgreementUpload = agreementPath,
            SapNo = dto.SapNo?.Trim(),
            PoNo = dto.PoNo?.Trim(),
            PfNo = dto.PfNo?.Trim(),
            PtCode = dto.PtCode?.Trim(),
            ContactorType = dto.ContractorType?.Trim(),
            PoValidFrom = dto.PoValidFrom,
            PoValidTo = dto.PoValidTo
        };

        _context.Contractors.Add(contractor);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("Contractor Master", "CREATE", nextId.ToString(), userId, cancellationToken);

        return MapToDetailDto(contractor, null);
    }

    public async Task<ContractorDetailDto?> UpdateContractorAsync(decimal id, ContractorCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default)
    {
        var contractor = await _context.Contractors
            .FirstOrDefaultAsync(c => c.ContractorId == id, cancellationToken);

        if (contractor == null)
        {
            return null;
        }

        // 1. Detailed duplicate validation excluding current contractor
        await ValidateDuplicatesAsync(dto, id, cancellationToken);

        // 2. Replace License Document if new file provided
        if (dto.LicenseDocument != null && dto.LicenseDocument.Length > 0)
        {
            DeleteFile(contractor.LicenseUpload);
            contractor.LicenseUpload = await SaveDocumentAsync(id, "license", dto.LicenseDocument, cancellationToken);
        }

        // 3. Replace Agreement Document if new file provided
        if (dto.AgreementDocument != null && dto.AgreementDocument.Length > 0)
        {
            DeleteFile(contractor.AgreementUpload);
            contractor.AgreementUpload = await SaveDocumentAsync(id, "agreement", dto.AgreementDocument, cancellationToken);
        }

        // 4. Update fields
        contractor.Name = dto.Name;
        contractor.Address = dto.Address?.Trim();
        contractor.Phone = dto.Phone?.Trim();
        contractor.Email = dto.Email?.Trim();
        contractor.Description = dto.Description?.Trim();
        contractor.LogDt = DateTime.Now;
        contractor.EsiPay = dto.EsiPay;
        contractor.PfPay = dto.PfPay;
        contractor.SalPayFlag = dto.SalPayFlag;
        contractor.ContPerNm = dto.ContPerNm?.Trim();
        contractor.ContPerPhone = dto.ContPerPhone?.Trim();
        contractor.ContPerAddress = dto.ContPerAddress?.Trim();
        contractor.StdLStrength = dto.StdLStrength;
        contractor.ValidDt = dto.ValidDt;
        contractor.LicNo = dto.LicNo;
        contractor.DocList = dto.DocList?.Trim();
        contractor.FaxNo = dto.FaxNo?.Trim();
        contractor.EsicNo = dto.EsicNo?.Trim();
        contractor.AdharNo = dto.AdharNo;
        contractor.PanNo = dto.PanNo;
        contractor.SapNo = dto.SapNo?.Trim();
        contractor.PoNo = dto.PoNo?.Trim();
        contractor.PfNo = dto.PfNo?.Trim();
        contractor.PtCode = dto.PtCode?.Trim();
        contractor.ContactorType = dto.ContractorType?.Trim();
        contractor.PoValidFrom = dto.PoValidFrom;
        contractor.PoValidTo = dto.PoValidTo;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("Contractor Master", "UPDATE", id.ToString(), userId, cancellationToken);

        return MapToDetailDto(contractor, null);
    }

    public async Task<bool> DeleteContractorAsync(decimal id, string? userId, CancellationToken cancellationToken = default)
    {
        var contractor = await _context.Contractors
            .FirstOrDefaultAsync(c => c.ContractorId == id, cancellationToken);

        if (contractor == null)
        {
            return false;
        }

        // Guard: Check EmployeeMaster for active employees (Status = 1) linked to this contractor
        var intId = (int)id;
        var hasActiveEmployees = await _context.EmployeeMasters
            .AnyAsync(e => e.LabContID == intId && e.Status == 1, cancellationToken);

        if (hasActiveEmployees)
        {
            throw new ContractorConflictException("Contractor", "Cannot delete: contractor has active employees");
        }

        // Clean up file storage
        DeleteContractorFolder(id);

        _context.Contractors.Remove(contractor);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("Contractor Master", "DELETE", id.ToString(), userId, cancellationToken);

        return true;
    }

    private async Task ValidateDuplicatesAsync(ContractorCreateUpdateDto dto, decimal? currentId, CancellationToken cancellationToken)
    {
        var nameLower = dto.Name.ToLower();
        var nameExists = await _context.Contractors
            .AnyAsync(c => (currentId == null || c.ContractorId != currentId) && c.Name.ToLower() == nameLower, cancellationToken);
        if (nameExists)
        {
            throw new ContractorConflictException("NAME", $"A contractor with the name '{dto.Name}' already exists.");
        }

        var licLower = dto.LicNo.ToLower();
        var licExists = await _context.Contractors
            .AnyAsync(c => (currentId == null || c.ContractorId != currentId) && c.LicNo != null && c.LicNo.ToLower() == licLower, cancellationToken);
        if (licExists)
        {
            throw new ContractorConflictException("LicNo", $"A contractor with License Number '{dto.LicNo}' already exists.");
        }

        var adharExists = await _context.Contractors
            .AnyAsync(c => (currentId == null || c.ContractorId != currentId) && c.AdharNo != null && c.AdharNo == dto.AdharNo, cancellationToken);
        if (adharExists)
        {
            throw new ContractorConflictException("AdharNo", $"A contractor with Aadhaar Number '{dto.AdharNo}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(dto.PanNo))
        {
            var panUpper = dto.PanNo.ToUpper();
            var panExists = await _context.Contractors
                .AnyAsync(c => (currentId == null || c.ContractorId != currentId) && c.PanNo != null && c.PanNo.ToUpper() == panUpper, cancellationToken);
            if (panExists)
            {
                throw new ContractorConflictException("PANNO", $"A contractor with PAN '{dto.PanNo}' already exists.");
            }
        }
    }

    private async Task<string> SaveDocumentAsync(decimal contractorId, string subFolder, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException($"Document '{file.FileName}' exceeds the maximum allowed size of 5 MB.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"File type '{ext}' is not allowed. Only PDF, JPG, and PNG documents are accepted.");
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "contractors", contractorId.ToString(), subFolder);

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(folder, safeFileName);

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"/uploads/contractors/{contractorId}/{subFolder}/{safeFileName}";
    }

    private void DeleteFile(string? relativeUrl)
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
            _logger.LogWarning(ex, "Could not delete document file: {Url}", relativeUrl);
        }
    }

    private void DeleteContractorFolder(decimal contractorId)
    {
        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "contractors", contractorId.ToString());

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete contractor folder for ContractorId: {ContractorId}", contractorId);
        }
    }

    private static ContractorDetailDto MapToDetailDto(ContractorMaster c, string? typeName)
    {
        return new ContractorDetailDto
        {
            ContractorId = c.ContractorId,
            Name = c.Name,
            Address = c.Address,
            Phone = c.Phone,
            Email = c.Email,
            Description = c.Description,
            LogId = c.LogId,
            LogDt = c.LogDt,
            EsiPay = c.EsiPay,
            PfPay = c.PfPay,
            SalPayFlag = c.SalPayFlag,
            ContPerNm = c.ContPerNm,
            ContPerPhone = c.ContPerPhone,
            ContPerAddress = c.ContPerAddress,
            StdLStrength = c.StdLStrength,
            ValidDt = c.ValidDt,
            LicNo = c.LicNo,
            DocList = c.DocList,
            FaxNo = c.FaxNo,
            EsicNo = c.EsicNo,
            AdharNo = c.AdharNo,
            PanNo = c.PanNo,
            LicenseUrl = c.LicenseUpload,
            AgreementUrl = c.AgreementUpload,
            SapNo = c.SapNo,
            PoNo = c.PoNo,
            PfNo = c.PfNo,
            PtCode = c.PtCode,
            ContractorType = c.ContactorType,
            ContractorTypeName = typeName,
            PoValidFrom = c.PoValidFrom,
            PoValidTo = c.PoValidTo
        };
    }
}
