using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLMS_APIs.Models.Entities;

[Table("ContractorMaster")]
public class ContractorMaster
{
    [Key]
    [Column("ContractorID", TypeName = "numeric(18,0)")]
    public decimal ContractorId { get; set; }

    [Required]
    [Column("NAME", TypeName = "nvarchar(250)")]
    [MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    [Column("Address", TypeName = "nvarchar(500)")]
    [MaxLength(500)]
    public string? Address { get; set; }

    [Column("Phone", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? Phone { get; set; }

    [Column("Email", TypeName = "varchar(100)")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Column("Description", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? Description { get; set; }

    [Column("LogId")]
    public int? LogId { get; set; }

    [Column("LogDt", TypeName = "datetime")]
    public DateTime? LogDt { get; set; }

    [Column("ESIPay")]
    public bool? EsiPay { get; set; }

    [Column("PFPay")]
    public bool? PfPay { get; set; }

    [Column("SalPayFlag")]
    public bool? SalPayFlag { get; set; }

    [Column("ContPerNm", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? ContPerNm { get; set; }

    [Column("ContPerPhone", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? ContPerPhone { get; set; }

    [Column("ContPerAddress", TypeName = "nvarchar(70)")]
    [MaxLength(70)]
    public string? ContPerAddress { get; set; }

    [Column("StdLStrength")]
    public int? StdLStrength { get; set; }

    [Column("ValidDt", TypeName = "datetime")]
    public DateTime? ValidDt { get; set; }

    [Column("LicNo", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? LicNo { get; set; }

    [Column("DocList", TypeName = "nvarchar(500)")]
    [MaxLength(500)]
    public string? DocList { get; set; }

    [Column("FaxNo", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? FaxNo { get; set; }

    [Column("ESIC_No", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? EsicNo { get; set; }

    [Column("AdharNo", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? AdharNo { get; set; }

    [Column("PANNO", TypeName = "nvarchar(50)")]
    [MaxLength(50)]
    public string? PanNo { get; set; }

    [Column("licenseupload", TypeName = "nvarchar(max)")]
    public string? LicenseUpload { get; set; }

    [Column("agreementupload", TypeName = "nvarchar(max)")]
    public string? AgreementUpload { get; set; }

    [Column("SapNo", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? SapNo { get; set; }

    [Column("PO_No", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? PoNo { get; set; }

    [Column("PF_No", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? PfNo { get; set; }

    [Column("PT_Code", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? PtCode { get; set; }

    [Column("ContactorType", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? ContactorType { get; set; }

    [Column("PO_ValidFrom", TypeName = "datetime")]
    public DateTime? PoValidFrom { get; set; }

    [Column("PO_ValidTo", TypeName = "datetime")]
    public DateTime? PoValidTo { get; set; }
}
