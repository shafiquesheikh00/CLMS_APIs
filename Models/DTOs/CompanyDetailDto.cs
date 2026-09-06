namespace CLMS_APIs.Models.DTOs;

public class CompanyDetailDto
{
    public string Id => CompanyId;
    public string CompanyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Address => CompanyAddress;
    public string CompanyAddress { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public DateTime? EntLog { get; set; }
    public DateTime? ModDt { get; set; }
    public string? Tin { get; set; }
    public string? Pan { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? Website { get; set; }
    public int? GraceTimeMinutes => GressTime;
    public int? GressTime { get; set; }
    public int? RegisteredFingerprintCount => RegFingerCount;
    public int? RegFingerCount { get; set; }
    public string? AlternateTelephone => AlternateTelNo?.ToString();
    public int? AlternateTelNo { get; set; }
}
