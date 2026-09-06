using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("CompanyMaster")]
public class CompanyMaster
{
    [Key]
    [Column("Company_ID", TypeName = "nvarchar(20)")]
    [MaxLength(20)]
    public string CompanyId { get; set; } = string.Empty;

    [Required]
    [Column("Company_Name", TypeName = "nvarchar(100)")]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [Column("Company_Address", TypeName = "nvarchar(300)")]
    [MaxLength(300)]
    public string CompanyAddress { get; set; } = string.Empty;

    [Column("Comp_Logo", TypeName = "nvarchar(max)")]
    public string? CompLogo { get; set; }

    [Column("Mod_Dt", TypeName = "datetime")]
    public DateTime? ModDt { get; set; }

    [Column("Ent_Log", TypeName = "datetime")]
    public DateTime? EntLog { get; set; }

    [Column("TIN", TypeName = "nvarchar(15)")]
    [MaxLength(15)]
    public string? Tin { get; set; }

    [Column("PAN", TypeName = "nvarchar(12)")]
    [MaxLength(12)]
    public string? Pan { get; set; }

    [Column("Phone", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? Phone { get; set; }

    [Column("Fax", TypeName = "nvarchar(30)")]
    [MaxLength(30)]
    public string? Fax { get; set; }

    [Column("Emailid", TypeName = "nvarchar(30)")]
    [MaxLength(30)]
    public string? EmailId { get; set; }

    [Column("ContactPreson", TypeName = "nvarchar(30)")]
    [MaxLength(30)]
    public string? ContactPerson { get; set; }

    [Column("WebSite", TypeName = "nvarchar(100)")]
    [MaxLength(100)]
    public string? Website { get; set; }

    [Column("GressTime")]
    public int? GressTime { get; set; }

    [Column("RegFingerCount")]
    public int? RegFingerCount { get; set; }

    [Column("AlternateTelNo")]
    public int? AlternateTelNo { get; set; }
}
