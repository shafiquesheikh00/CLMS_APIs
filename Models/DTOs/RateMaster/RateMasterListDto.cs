namespace CLMS_APIs.Models.DTOs.RateMaster;

public class RateMasterListDto
{
    public decimal Rid { get; set; }
    public int? LabourCatId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? RdateFrom { get; set; }
    public DateTime? Rdateto { get; set; }
    public decimal? RatePerDay { get; set; }
    public decimal? RateOTPerHour { get; set; }
    public decimal? Basic { get; set; }
    public decimal? SpecialAllowance { get; set; }
    public decimal? Hra { get; set; }
    public decimal? OtherAllowance { get; set; }
    public decimal? Gross { get; set; }
    public string? EmpCategoryFlag { get; set; }
}
