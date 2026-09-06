using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CLMS_APIs.Models.DTOs;

public class CompanyCreateUpdateDto
{
    private string _companyName = string.Empty;
    private string _companyAddress = string.Empty;
    private int? _gressTime;
    private int? _regFingerCount;
    private int? _alternateTelNo;
    private IFormFile? _logo;

    [Required(ErrorMessage = "Company Name is required.")]
    [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters.")]
    public string CompanyName
    {
        get => _companyName;
        set { if (!string.IsNullOrWhiteSpace(value)) _companyName = value; }
    }

    // Alias for frontend 'name' or 'company_name'
    public string? Company_Name
    {
        get => _companyName;
        set { if (!string.IsNullOrWhiteSpace(value)) _companyName = value; }
    }

    [Required(ErrorMessage = "Company Address is required.")]
    [StringLength(300, ErrorMessage = "Company Address cannot exceed 300 characters.")]
    public string CompanyAddress
    {
        get => _companyAddress;
        set { if (!string.IsNullOrWhiteSpace(value)) _companyAddress = value; }
    }

    // Alias for frontend 'address'
    public string? Address
    {
        get => _companyAddress;
        set { if (!string.IsNullOrWhiteSpace(value)) _companyAddress = value; }
    }

    // Alias for 'company_address'
    public string? Company_Address
    {
        get => _companyAddress;
        set { if (!string.IsNullOrWhiteSpace(value)) _companyAddress = value; }
    }

    [StringLength(15, ErrorMessage = "TIN cannot exceed 15 characters.")]
    public string? Tin { get; set; }

    [StringLength(12, ErrorMessage = "PAN cannot exceed 12 characters.")]
    public string? Pan { get; set; }

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters.")]
    public string? Phone { get; set; }

    [StringLength(30, ErrorMessage = "Fax cannot exceed 30 characters.")]
    public string? Fax { get; set; }

    [StringLength(30, ErrorMessage = "Email cannot exceed 30 characters.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
    public string? Email { get; set; }

    [StringLength(30, ErrorMessage = "Contact Person cannot exceed 30 characters.")]
    public string? ContactPerson { get; set; }

    [StringLength(100, ErrorMessage = "Website cannot exceed 100 characters.")]
    public string? Website { get; set; }

    [Range(0, 1440, ErrorMessage = "Grace Time must be between 0 and 1440 minutes.")]
    public int? GressTime
    {
        get => _gressTime;
        set => _gressTime = value;
    }

    // Alias for frontend 'graceTimeMinutes'
    public int? GraceTimeMinutes
    {
        get => _gressTime;
        set => _gressTime = value;
    }

    [Range(0, 10, ErrorMessage = "Registered Finger Count must be between 0 and 10.")]
    public int? RegFingerCount
    {
        get => _regFingerCount;
        set => _regFingerCount = value;
    }

    // Alias for frontend 'registeredFingerprintCount'
    public int? RegisteredFingerprintCount
    {
        get => _regFingerCount;
        set => _regFingerCount = value;
    }

    public int? AlternateTelNo
    {
        get => _alternateTelNo;
        set => _alternateTelNo = value;
    }

    // Alias for frontend 'alternateTelephone'
    public string? AlternateTelephone
    {
        get => _alternateTelNo?.ToString();
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (int.TryParse(value, out var n))
                {
                    _alternateTelNo = n;
                }
                else
                {
                    var digits = new string(value.Where(char.IsDigit).ToArray());
                    if (int.TryParse(digits, out var parsedDigits))
                    {
                        _alternateTelNo = parsedDigits;
                    }
                }
            }
        }
    }

    public IFormFile? Logo
    {
        get => _logo;
        set => _logo = value;
    }

    // Alias for frontend 'logoFile'
    public IFormFile? LogoFile
    {
        get => _logo;
        set => _logo = value;
    }
}
