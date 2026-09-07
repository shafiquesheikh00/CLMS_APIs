namespace CLMS_APIs.Models.DTOs.RateMaster;

public class RateMasterDetailDto
{
    public decimal Rid { get; set; }
    public DateTime? RdateFrom { get; set; }
    public DateTime? Rdateto { get; set; }
    public int? LabourCatId { get; set; }
    public string? CategoryName { get; set; }
    public decimal? RatePerDay { get; set; }
    public decimal? RateOTPerHour { get; set; }
    public decimal? Basic { get; set; }
    public decimal? SpecialAllowance { get; set; }
    public decimal? Hra { get; set; }
    public decimal? OtherAllowance { get; set; }
    public int? LogId { get; set; }
    public DateTime? LogDt { get; set; }
    public decimal? Hraper { get; set; }
    public decimal? BonusPer { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Lww { get; set; }
    public decimal? Gross { get; set; }
    public decimal? Pfper { get; set; }
    public decimal? Pf { get; set; }
    public int? AttendanceAllowAppAfter { get; set; }
    public decimal? AttendanceAllowRs { get; set; }
    public decimal? Da { get; set; }
    public decimal? Daper { get; set; }
    public decimal? PF_Deduction { get; set; }
    public decimal? Esi { get; set; }
    public decimal? Pt { get; set; }
    public decimal? Advance { get; set; }
    public decimal? Lic { get; set; }
    public decimal? Lwf { get; set; }
    public decimal? EducationAllowance { get; set; }
    public decimal? OtherAll { get; set; }
    public int? AttendanceAllowAppAfter2 { get; set; }
    public decimal? AttendanceAllowRs2 { get; set; }
    public bool Pfapply { get; set; }
    public bool Esicapply { get; set; }
    public bool Ptapply { get; set; }
    public decimal? AttendanceAllowRs3 { get; set; }
    public int? AttendanceAllowAppAfter3 { get; set; }
    public decimal? Stipend { get; set; }
    public decimal? ServiceCharge { get; set; }
    public string? EmpCategoryFlag { get; set; }
}
