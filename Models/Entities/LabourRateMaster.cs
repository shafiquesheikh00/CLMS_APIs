using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("LabourRateMaster")]
public class LabourRateMaster
{
    [Key]
    [Column("RID", TypeName = "numeric(18,0)")]
    public decimal Rid { get; set; }

    [Column("RdateFrom", TypeName = "smalldatetime")]
    public DateTime? RdateFrom { get; set; }

    [Column("Rdateto", TypeName = "smalldatetime")]
    public DateTime? Rdateto { get; set; }

    [Column("LabourCatID")]
    public int? LabourCatId { get; set; }

    [Column("RatePerDay", TypeName = "numeric(18,2)")]
    public decimal? RatePerDay { get; set; }

    [Column("RateOTPerHour", TypeName = "numeric(18,2)")]
    public decimal? RateOTPerHour { get; set; }

    [Column("Basic", TypeName = "decimal(18,2)")]
    public decimal? Basic { get; set; }

    [Column("Special_Allowance", TypeName = "decimal(18,2)")]
    public decimal? Special_Allowance { get; set; }

    [Column("HRA", TypeName = "decimal(18,2)")]
    public decimal? Hra { get; set; }

    [Column("Other_Allowance", TypeName = "decimal(18,2)")]
    public decimal? Other_Allowance { get; set; }

    [Column("LogID")]
    public int? LogId { get; set; }

    [Column("LogDt", TypeName = "smalldatetime")]
    public DateTime? LogDt { get; set; }

    [Column("HRAPER", TypeName = "decimal(18,2)")]
    public decimal? Hraper { get; set; }

    [Column("BonusPER", TypeName = "decimal(18,2)")]
    public decimal? BonusPer { get; set; }

    [Column("Bonus", TypeName = "decimal(18,2)")]
    public decimal? Bonus { get; set; }

    [Column("LWW", TypeName = "decimal(18,2)")]
    public decimal? Lww { get; set; }

    [Column("gross", TypeName = "decimal(18,2)")]
    public decimal? Gross { get; set; }

    [Column("PFPER", TypeName = "decimal(18,2)")]
    public decimal? Pfper { get; set; }

    [Column("PF", TypeName = "decimal(18,2)")]
    public decimal? Pf { get; set; }

    [Column("Attendance_Allow_App_After")]
    public int? Attendance_Allow_App_After { get; set; }

    [Column("Attendance_Allow_Rs", TypeName = "decimal(18,2)")]
    public decimal? Attendance_Allow_Rs { get; set; }

    [Column("DA", TypeName = "decimal(18,2)")]
    public decimal? Da { get; set; }

    [Column("DAPER", TypeName = "decimal(18,2)")]
    public decimal? Daper { get; set; }

    [Column("P_F", TypeName = "decimal(18,2)")]
    public decimal? P_F { get; set; }

    [Column("ESI", TypeName = "decimal(18,2)")]
    public decimal? Esi { get; set; }

    [Column("PT", TypeName = "decimal(18,2)")]
    public decimal? Pt { get; set; }

    [Column("Advance", TypeName = "decimal(18,2)")]
    public decimal? Advance { get; set; }

    [Column("LIC", TypeName = "decimal(18,2)")]
    public decimal? Lic { get; set; }

    [Column("LWF", TypeName = "decimal(18,2)")]
    public decimal? Lwf { get; set; }

    [Column("EducationAllowance", TypeName = "decimal(18,0)")]
    public decimal? EducationAllowance { get; set; }

    [Column("Other_All", TypeName = "decimal(18,0)")]
    public decimal? Other_All { get; set; }

    [Column("Attendance_Allow_App_After2")]
    public int? Attendance_Allow_App_After2 { get; set; }

    [Column("Attendance_Allow_Rs2", TypeName = "decimal(18,2)")]
    public decimal? Attendance_Allow_Rs2 { get; set; }

    [Column("PFApply", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? Pfapply { get; set; }

    [Column("ESICApply", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? Esicapply { get; set; }

    [Column("PTApply", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? Ptapply { get; set; }

    [Column("Attendance_Allow_Rs3", TypeName = "decimal(18,2)")]
    public decimal? Attendance_Allow_Rs3 { get; set; }

    [Column("Attendance_Allow_App_After3")]
    public int? Attendance_Allow_App_After3 { get; set; }

    [Column("Stipend", TypeName = "decimal(18,2)")]
    public decimal? Stipend { get; set; }

    [Column("ServiceCharge", TypeName = "decimal(18,0)")]
    public decimal? ServiceCharge { get; set; }

    [Column("EmpCategoryFlag", TypeName = "nvarchar(10)")]
    [MaxLength(10)]
    public string? EmpCategoryFlag { get; set; }
}
