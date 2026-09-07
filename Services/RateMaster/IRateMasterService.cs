using CLMS_APIs.Models.DTOs.RateMaster;

namespace CLMS_APIs.Services.RateMaster;

public interface IRateMasterService
{
    Task<PagedResult<RateMasterListDto>> GetRatesAsync(RateMasterQueryRequest query, CancellationToken cancellationToken = default);
    Task<RateMasterDetailDto?> GetRateByIdAsync(decimal rid, CancellationToken cancellationToken = default);
    Task<List<RateMasterCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<RateMasterDetailDto> CreateRateAsync(CreateRateMasterRequest request, string? userId, CancellationToken cancellationToken = default);
    Task<RateMasterDetailDto?> UpdateRateAsync(decimal rid, UpdateRateMasterRequest request, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRateAsync(decimal rid, string? categoryType, string? userId, CancellationToken cancellationToken = default);
    (decimal Gross, decimal RatePerDay) CalculateGrossAndRate(
        decimal basic,
        decimal stipend,
        decimal hra,
        decimal da,
        decimal bonus,
        decimal specialAllowance,
        decimal otherAllowance,
        decimal educationAllowance,
        decimal otherAll);
}
