namespace CLMS_APIs.Models.DTOs;

public class ContractorListDto
{
    public decimal Id => ContractorId;
    public decimal ContractorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AdharNo { get; set; }
    public string? AadharNo => AdharNo;
    public int? StdLStrength { get; set; }
    public DateTime? ValidDt { get; set; }
    public bool IsActive => ValidDt.HasValue && ValidDt.Value.Date >= DateTime.Today;
}
