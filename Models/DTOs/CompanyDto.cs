namespace CLMS_APIs.Models.DTOs;

public class CompanyDto
{
    public string Id => CompanyId;
    public string CompanyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Address => CompanyAddress;
    public string CompanyAddress { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AlternateTelephone => AlternateTelNo?.ToString();
    public int? AlternateTelNo { get; set; }
    public string? LogoUrl { get; set; }
}
