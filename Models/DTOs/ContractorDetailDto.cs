namespace CLMS_APIs.Models.DTOs;

public class ContractorDetailDto
{
    public decimal Id => ContractorId;
    public decimal ContractorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public int? LogId { get; set; }
    public DateTime? LogDt { get; set; }
    public bool? EsiPay { get; set; }
    public bool? PfPay { get; set; }
    public bool? SalPayFlag { get; set; }
    public string? ContPerNm { get; set; }
    public string? ContPerPhone { get; set; }
    public string? ContPerAddress { get; set; }
    public int? StdLStrength { get; set; }
    public DateTime? ValidDt { get; set; }
    public bool IsActive => ValidDt.HasValue && ValidDt.Value.Date >= DateTime.Today;
    public string? LicNo { get; set; }
    public string? DocList { get; set; }
    public string? FaxNo { get; set; }
    public string? EsicNo { get; set; }
    public string? AdharNo { get; set; }
    public string? AadharNo => AdharNo;
    public string? PanNo { get; set; }
    public string? LicenseUrl { get; set; }
    public string? AgreementUrl { get; set; }
    public string? SapNo { get; set; }
    public string? PoNo { get; set; }
    public string? PfNo { get; set; }
    public string? PtCode { get; set; }
    public string? ContractorType { get; set; }
    public string? ContractorTypeName { get; set; }
    public DateTime? PoValidFrom { get; set; }
    public DateTime? PoValidTo { get; set; }
}
