using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CLMS_APIs.Models.DTOs;

public class ContractorCreateUpdateDto
{
    private string _name = string.Empty;
    private string _licNo = string.Empty;
    private string _adharNo = string.Empty;
    private string? _panNo;
    private string? _contractorType;
    private IFormFile? _licenseDocument;
    private IFormFile? _agreementDocument;

    [Required(ErrorMessage = "Contractor Name is required.")]
    [StringLength(250, ErrorMessage = "Contractor Name cannot exceed 250 characters.")]
    public string Name
    {
        get => _name;
        set { if (!string.IsNullOrWhiteSpace(value)) _name = value.Trim(); }
    }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters.")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [EmailAddress(ErrorMessage = "Invalid Email address format.")]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "Description cannot exceed 50 characters.")]
    public string? Description { get; set; }

    public bool? EsiPay { get; set; }
    public bool? PfPay { get; set; }
    public bool? SalPayFlag { get; set; }

    [StringLength(50, ErrorMessage = "Contact Person Name cannot exceed 50 characters.")]
    public string? ContPerNm { get; set; }

    [StringLength(50, ErrorMessage = "Contact Person Phone cannot exceed 50 characters.")]
    public string? ContPerPhone { get; set; }

    [StringLength(70, ErrorMessage = "Contact Person Address cannot exceed 70 characters.")]
    public string? ContPerAddress { get; set; }

    public int? StdLStrength { get; set; }

    public DateTime? ValidDt { get; set; }

    [Required(ErrorMessage = "License Number is required.")]
    [StringLength(50, ErrorMessage = "License Number cannot exceed 50 characters.")]
    public string LicNo
    {
        get => _licNo;
        set { if (!string.IsNullOrWhiteSpace(value)) _licNo = value.Trim(); }
    }

    [StringLength(500, ErrorMessage = "Document List cannot exceed 500 characters.")]
    public string? DocList { get; set; }

    [StringLength(50, ErrorMessage = "Fax Number cannot exceed 50 characters.")]
    public string? FaxNo { get; set; }

    [StringLength(50, ErrorMessage = "ESIC Number cannot exceed 50 characters.")]
    public string? EsicNo { get; set; }

    [Required(ErrorMessage = "Aadhaar Number is required.")]
    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhaar Number must be exactly 12 digits.")]
    public string AdharNo
    {
        get => _adharNo;
        set { if (!string.IsNullOrWhiteSpace(value)) _adharNo = value.Trim(); }
    }

    // Alias for frontend 'aadharNo'
    public string? AadharNo
    {
        get => _adharNo;
        set { if (!string.IsNullOrWhiteSpace(value)) _adharNo = value.Trim(); }
    }

    [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN must be in valid 10-character format (e.g. ABCDE1234F).")]
    public string? PanNo
    {
        get => _panNo;
        set { if (!string.IsNullOrWhiteSpace(value)) _panNo = value.Trim().ToUpperInvariant(); }
    }

    // Alias for 'PANNO'
    public string? PANNO
    {
        get => _panNo;
        set { if (!string.IsNullOrWhiteSpace(value)) _panNo = value.Trim().ToUpperInvariant(); }
    }

    [StringLength(50, ErrorMessage = "SAP Number cannot exceed 50 characters.")]
    public string? SapNo { get; set; }

    [StringLength(50, ErrorMessage = "PO Number cannot exceed 50 characters.")]
    public string? PoNo { get; set; }

    [StringLength(50, ErrorMessage = "PF Number cannot exceed 50 characters.")]
    public string? PfNo { get; set; }

    [StringLength(50, ErrorMessage = "PT Code cannot exceed 50 characters.")]
    public string? PtCode { get; set; }

    [StringLength(50, ErrorMessage = "Contractor Type cannot exceed 50 characters.")]
    public string? ContractorType
    {
        get => _contractorType;
        set { if (!string.IsNullOrWhiteSpace(value)) _contractorType = value.Trim(); }
    }

    // Alias for legacy DB column typo 'ContactorType'
    public string? ContactorType
    {
        get => _contractorType;
        set { if (!string.IsNullOrWhiteSpace(value)) _contractorType = value.Trim(); }
    }

    public DateTime? PoValidFrom { get; set; }
    public DateTime? PoValidTo { get; set; }

    public IFormFile? LicenseDocument
    {
        get => _licenseDocument;
        set => _licenseDocument = value;
    }

    // Alias for frontend 'licenseFile'
    public IFormFile? LicenseFile
    {
        get => _licenseDocument;
        set => _licenseDocument = value;
    }

    public IFormFile? AgreementDocument
    {
        get => _agreementDocument;
        set => _agreementDocument = value;
    }

    // Alias for frontend 'agreementFile'
    public IFormFile? AgreementFile
    {
        get => _agreementDocument;
        set => _agreementDocument = value;
    }
}
