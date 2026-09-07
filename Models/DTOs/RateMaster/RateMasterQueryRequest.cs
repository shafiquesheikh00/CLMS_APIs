namespace CLMS_APIs.Models.DTOs.RateMaster;

public class RateMasterQueryRequest
{
    public string? CategoryType { get; set; }
    public int? LabourCatId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "RID";
    public string? SortDirection { get; set; } = "DESC";
}
