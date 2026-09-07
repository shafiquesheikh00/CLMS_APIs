using System.Data;
using CLMS_APIs.Data;
using CLMS_APIs.Exceptions;
using CLMS_APIs.Models.DTOs.RateMaster;
using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services.RateMaster;

public class RateMasterService : IRateMasterService
{
    private readonly ClmsDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<RateMasterService> _logger;

    public RateMasterService(
        ClmsDbContext context,
        IAuditLogService auditLogService,
        ILogger<RateMasterService> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<List<RateMasterCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OtherMasters
            .AsNoTracking()
            .Where(o => o.MasterName == "Labour Type Master" && (o.Status == null || o.Status == true))
            .OrderBy(o => o.MasterType)
            .Select(o => new RateMasterCategoryDto
            {
                Id = o.MasterTypeId,
                Name = o.MasterType
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<RateMasterListDto>> GetRatesAsync(RateMasterQueryRequest query, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 200 ? 200 : query.PageSize);

        var queryable = from r in _context.LabourRateMasters.AsNoTracking()
                        join o in _context.OtherMasters.AsNoTracking().Where(x => x.MasterName == "Labour Type Master")
                        on r.LabourCatId equals o.MasterTypeId into catGroup
                        from cat in catGroup.DefaultIfEmpty()
                        select new
                        {
                            Rate = r,
                            CategoryName = cat != null ? cat.MasterType : null
                        };

        // 1. Filter by Category Type (Labour vs Employee)
        if (!string.IsNullOrWhiteSpace(query.CategoryType))
        {
            var catType = query.CategoryType.Trim();
            queryable = queryable.Where(x => x.Rate.EmpCategoryFlag != null &&
                                             x.Rate.EmpCategoryFlag.ToLower() == catType.ToLower());
        }

        // 2. Filter by Labour Category ID
        if (query.LabourCatId.HasValue && query.LabourCatId.Value > 0)
        {
            queryable = queryable.Where(x => x.Rate.LabourCatId == query.LabourCatId.Value);
        }

        // 3. Filter by Date range
        if (query.FromDate.HasValue)
        {
            queryable = queryable.Where(x => x.Rate.RdateFrom >= query.FromDate.Value);
        }
        if (query.ToDate.HasValue)
        {
            queryable = queryable.Where(x => x.Rate.Rdateto <= query.ToDate.Value);
        }

        // 4. Free text search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(x => (x.CategoryName != null && x.CategoryName.ToLower().Contains(search)) ||
                                             (x.Rate.EmpCategoryFlag != null && x.Rate.EmpCategoryFlag.ToLower().Contains(search)) ||
                                             x.Rate.Rid.ToString().Contains(search));
        }

        // 5. Total Count
        var totalCount = await queryable.CountAsync(cancellationToken);

        // 6. Sorting
        var isAscending = string.Equals(query.SortDirection, "ASC", StringComparison.OrdinalIgnoreCase);
        var sortBy = (query.SortBy ?? "RID").ToUpperInvariant();

        queryable = (sortBy, isAscending) switch
        {
            ("RID", true) => queryable.OrderBy(x => x.Rate.Rid),
            ("RID", false) => queryable.OrderByDescending(x => x.Rate.Rid),
            ("FROMDATE", true) => queryable.OrderBy(x => x.Rate.RdateFrom),
            ("FROMDATE", false) => queryable.OrderByDescending(x => x.Rate.RdateFrom),
            ("TODATE", true) => queryable.OrderBy(x => x.Rate.Rdateto),
            ("TODATE", false) => queryable.OrderByDescending(x => x.Rate.Rdateto),
            ("RATEPERDAY", true) => queryable.OrderBy(x => x.Rate.RatePerDay),
            ("RATEPERDAY", false) => queryable.OrderByDescending(x => x.Rate.RatePerDay),
            ("GROSS", true) => queryable.OrderBy(x => x.Rate.Gross),
            ("GROSS", false) => queryable.OrderByDescending(x => x.Rate.Gross),
            ("CATEGORYNAME", true) => queryable.OrderBy(x => x.CategoryName),
            ("CATEGORYNAME", false) => queryable.OrderByDescending(x => x.CategoryName),
            _ => queryable.OrderByDescending(x => x.Rate.Rid)
        };

        // 7. Paginated Select
        var items = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RateMasterListDto
            {
                Rid = x.Rate.Rid,
                LabourCatId = x.Rate.LabourCatId,
                CategoryName = x.CategoryName,
                RdateFrom = x.Rate.RdateFrom,
                Rdateto = x.Rate.Rdateto,
                RatePerDay = x.Rate.RatePerDay,
                RateOTPerHour = x.Rate.RateOTPerHour,
                Basic = x.Rate.Basic,
                SpecialAllowance = x.Rate.Special_Allowance,
                Hra = x.Rate.Hra,
                OtherAllowance = x.Rate.Other_Allowance,
                Gross = x.Rate.Gross,
                EmpCategoryFlag = x.Rate.EmpCategoryFlag
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RateMasterListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<RateMasterDetailDto?> GetRateByIdAsync(decimal rid, CancellationToken cancellationToken = default)
    {
        var result = await (from r in _context.LabourRateMasters.AsNoTracking()
                            where r.Rid == rid
                            join o in _context.OtherMasters.AsNoTracking().Where(x => x.MasterName == "Labour Type Master")
                            on r.LabourCatId equals o.MasterTypeId into catGroup
                            from cat in catGroup.DefaultIfEmpty()
                            select new
                            {
                                Rate = r,
                                CategoryName = cat != null ? cat.MasterType : null
                            }).FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return null;
        }

        var rate = result.Rate;
        return new RateMasterDetailDto
        {
            Rid = rate.Rid,
            RdateFrom = rate.RdateFrom,
            Rdateto = rate.Rdateto,
            LabourCatId = rate.LabourCatId,
            CategoryName = result.CategoryName,
            RatePerDay = rate.RatePerDay,
            RateOTPerHour = rate.RateOTPerHour,
            Basic = rate.Basic,
            SpecialAllowance = rate.Special_Allowance,
            Hra = rate.Hra,
            OtherAllowance = rate.Other_Allowance,
            LogId = rate.LogId,
            LogDt = rate.LogDt,
            Hraper = rate.Hraper,
            BonusPer = rate.BonusPer,
            Bonus = rate.Bonus,
            Lww = rate.Lww,
            Gross = rate.Gross,
            Pfper = rate.Pfper,
            Pf = rate.Pf,
            AttendanceAllowAppAfter = rate.Attendance_Allow_App_After,
            AttendanceAllowRs = rate.Attendance_Allow_Rs,
            Da = rate.Da,
            Daper = rate.Daper,
            PF_Deduction = rate.P_F,
            Esi = rate.Esi,
            Pt = rate.Pt,
            Advance = rate.Advance,
            Lic = rate.Lic,
            Lwf = rate.Lwf,
            EducationAllowance = rate.EducationAllowance,
            OtherAll = rate.Other_All,
            AttendanceAllowAppAfter2 = rate.Attendance_Allow_App_After2,
            AttendanceAllowRs2 = rate.Attendance_Allow_Rs2,
            Pfapply = string.Equals(rate.Pfapply, "True", StringComparison.OrdinalIgnoreCase),
            Esicapply = string.Equals(rate.Esicapply, "True", StringComparison.OrdinalIgnoreCase),
            Ptapply = string.Equals(rate.Ptapply, "True", StringComparison.OrdinalIgnoreCase),
            AttendanceAllowRs3 = rate.Attendance_Allow_Rs3,
            AttendanceAllowAppAfter3 = rate.Attendance_Allow_App_After3,
            Stipend = rate.Stipend,
            ServiceCharge = rate.ServiceCharge,
            EmpCategoryFlag = rate.EmpCategoryFlag
        };
    }

    public async Task<RateMasterDetailDto> CreateRateAsync(CreateRateMasterRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        var catId = request.LabourCatId!.Value;
        var fromDate = request.RdateFrom!.Value;
        var toDate = request.Rdateto!.Value;
        var empCategory = NormalizeCategoryFlag(request.EmpCategoryFlag);

        // 1. Date overlap validation
        await ValidateOverlapAsync(catId, empCategory, fromDate, toDate, null, cancellationToken);

        // 2. Calculate Gross and RatePerDay on server side (never trust client)
        var (calculatedGross, calculatedRatePerDay) = CalculateGrossAndRate(
            request.Basic ?? 0m,
            request.Stipend ?? 0m,
            request.Hra ?? 0m,
            request.Da ?? 0m,
            request.Bonus ?? 0m,
            request.SpecialAllowance ?? 0m,
            request.OtherAllowance ?? 0m,
            request.EducationAllowance ?? 0m,
            request.OtherAll ?? 0m);

        int? parsedUserId = int.TryParse(userId, out var uid) ? uid : null;
        DateTime now = DateTime.Now;

        // 3. Concurrency-safe RID generation using Serializable database transaction
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var maxRid = await _context.LabourRateMasters
            .Select(r => (decimal?)r.Rid)
            .MaxAsync(cancellationToken) ?? 0m;

        var nextRid = maxRid + 1m;

        var entity = new LabourRateMaster
        {
            Rid = nextRid,
            RdateFrom = fromDate,
            Rdateto = toDate,
            LabourCatId = catId,
            RatePerDay = calculatedRatePerDay,
            RateOTPerHour = request.RateOTPerHour,
            Basic = request.Basic,
            Special_Allowance = request.SpecialAllowance,
            Hra = request.Hra,
            Other_Allowance = request.OtherAllowance,
            LogId = parsedUserId,
            LogDt = now,
            Hraper = request.Hraper,
            BonusPer = request.BonusPer,
            Bonus = request.Bonus,
            Lww = request.Lww,
            Gross = calculatedGross,
            Pfper = request.Pfper,
            Pf = request.Pf,
            Attendance_Allow_App_After = request.AttendanceAllowAppAfter,
            Attendance_Allow_Rs = request.AttendanceAllowRs,
            Da = request.Da,
            Daper = request.Daper,
            P_F = request.PF_Deduction,
            Esi = request.Esi,
            Pt = request.Pt,
            Advance = request.Advance,
            Lic = request.Lic,
            Lwf = request.Lwf,
            EducationAllowance = request.EducationAllowance,
            Other_All = request.OtherAll,
            Attendance_Allow_App_After2 = request.AttendanceAllowAppAfter2,
            Attendance_Allow_Rs2 = request.AttendanceAllowRs2,
            Pfapply = (request.Pfapply == true) ? "True" : "False",
            Esicapply = (request.Esicapply == true) ? "True" : "False",
            Ptapply = (request.Ptapply == true) ? "True" : "False",
            Attendance_Allow_Rs3 = request.AttendanceAllowRs3,
            Attendance_Allow_App_After3 = request.AttendanceAllowAppAfter3,
            Stipend = request.Stipend,
            ServiceCharge = request.ServiceCharge,
            EmpCategoryFlag = empCategory
        };

        _context.LabourRateMasters.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // 4. Audit Log
        await _auditLogService.LogAsync("Rate Master", "ADD", entity.Rid.ToString(), userId, cancellationToken);

        return (await GetRateByIdAsync(entity.Rid, cancellationToken))!;
    }

    public async Task<RateMasterDetailDto?> UpdateRateAsync(decimal rid, UpdateRateMasterRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.LabourRateMasters
            .FirstOrDefaultAsync(r => r.Rid == rid, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        var catId = request.LabourCatId!.Value;
        var fromDate = request.RdateFrom!.Value;
        var toDate = request.Rdateto!.Value;
        var empCategory = NormalizeCategoryFlag(request.EmpCategoryFlag);

        // 1. Overlap validation (excluding current RID)
        await ValidateOverlapAsync(catId, empCategory, fromDate, toDate, rid, cancellationToken);

        // 2. Recalculate Gross and RatePerDay
        var (calculatedGross, calculatedRatePerDay) = CalculateGrossAndRate(
            request.Basic ?? 0m,
            request.Stipend ?? 0m,
            request.Hra ?? 0m,
            request.Da ?? 0m,
            request.Bonus ?? 0m,
            request.SpecialAllowance ?? 0m,
            request.OtherAllowance ?? 0m,
            request.EducationAllowance ?? 0m,
            request.OtherAll ?? 0m);

        int? parsedUserId = int.TryParse(userId, out var uid) ? uid : entity.LogId;

        entity.RdateFrom = fromDate;
        entity.Rdateto = toDate;
        entity.LabourCatId = catId;
        entity.RatePerDay = calculatedRatePerDay;
        entity.RateOTPerHour = request.RateOTPerHour;
        entity.Basic = request.Basic;
        entity.Special_Allowance = request.SpecialAllowance;
        entity.Hra = request.Hra;
        entity.Other_Allowance = request.OtherAllowance;
        entity.LogId = parsedUserId;
        entity.LogDt = DateTime.Now;
        entity.Hraper = request.Hraper;
        entity.BonusPer = request.BonusPer;
        entity.Bonus = request.Bonus;
        entity.Lww = request.Lww;
        entity.Gross = calculatedGross;
        entity.Pfper = request.Pfper;
        entity.Pf = request.Pf;
        entity.Attendance_Allow_App_After = request.AttendanceAllowAppAfter;
        entity.Attendance_Allow_Rs = request.AttendanceAllowRs;
        entity.Da = request.Da;
        entity.Daper = request.Daper;
        entity.P_F = request.PF_Deduction;
        entity.Esi = request.Esi;
        entity.Pt = request.Pt;
        entity.Advance = request.Advance;
        entity.Lic = request.Lic;
        entity.Lwf = request.Lwf;
        entity.EducationAllowance = request.EducationAllowance;
        entity.Other_All = request.OtherAll;
        entity.Attendance_Allow_App_After2 = request.AttendanceAllowAppAfter2;
        entity.Attendance_Allow_Rs2 = request.AttendanceAllowRs2;
        entity.Pfapply = (request.Pfapply == true) ? "True" : "False";
        entity.Esicapply = (request.Esicapply == true) ? "True" : "False";
        entity.Ptapply = (request.Ptapply == true) ? "True" : "False";
        entity.Attendance_Allow_Rs3 = request.AttendanceAllowRs3;
        entity.Attendance_Allow_App_After3 = request.AttendanceAllowAppAfter3;
        entity.Stipend = request.Stipend;
        entity.ServiceCharge = request.ServiceCharge;
        entity.EmpCategoryFlag = empCategory;

        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Rate Master", "UPDATE", rid.ToString(), userId, cancellationToken);

        return await GetRateByIdAsync(rid, cancellationToken);
    }

    public async Task<bool> DeleteRateAsync(decimal rid, string? categoryType, string? userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.LabourRateMasters
            .FirstOrDefaultAsync(r => r.Rid == rid, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        // Safety check if categoryType was explicitly provided
        if (!string.IsNullOrWhiteSpace(categoryType))
        {
            var normalized = NormalizeCategoryFlag(categoryType);
            if (!string.Equals(entity.EmpCategoryFlag, normalized, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Rate master record '{rid}' belongs to category '{entity.EmpCategoryFlag}', not '{categoryType}'.");
            }
        }

        _context.LabourRateMasters.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.LogAsync("Rate Master", "DELETE", rid.ToString(), userId, cancellationToken);

        return true;
    }

    /// <summary>
    /// Computes Gross and RatePerDay server-side as the sum of all allowance & basic components.
    /// </summary>
    public (decimal Gross, decimal RatePerDay) CalculateGrossAndRate(
        decimal basic,
        decimal stipend,
        decimal hra,
        decimal da,
        decimal bonus,
        decimal specialAllowance,
        decimal otherAllowance,
        decimal educationAllowance,
        decimal otherAll)
    {
        decimal total = basic
                      + stipend
                      + hra
                      + da
                      + bonus
                      + specialAllowance
                      + otherAllowance
                      + educationAllowance
                      + otherAll;

        decimal rounded = Math.Round(total, 2);
        return (rounded, rounded);
    }

    private async Task ValidateOverlapAsync(
        int labourCatId, 
        string empCategoryFlag, 
        DateTime fromDate, 
        DateTime toDate, 
        decimal? excludeRid, 
        CancellationToken cancellationToken)
    {
        var overlapExists = await _context.LabourRateMasters
            .AsNoTracking()
            .AnyAsync(r => (!excludeRid.HasValue || r.Rid != excludeRid.Value) &&
                           r.LabourCatId == labourCatId &&
                           r.EmpCategoryFlag != null &&
                           r.EmpCategoryFlag.ToLower() == empCategoryFlag.ToLower() &&
                           r.RdateFrom.HasValue && r.Rdateto.HasValue &&
                           r.RdateFrom.Value <= toDate &&
                           r.Rdateto.Value >= fromDate,
                      cancellationToken);

        if (overlapExists)
        {
            throw new DuplicateEntityException($"A rate already exists for category ID '{labourCatId}' ({empCategoryFlag}) overlapping with the date range {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}.");
        }
    }

    private static string NormalizeCategoryFlag(string flag)
    {
        if (string.Equals(flag, "Employee", StringComparison.OrdinalIgnoreCase))
            return "Employee";
        return "Labour";
    }
}
