using CLMS_APIs.Models.DTOs;

namespace CLMS_APIs.Services;

public interface IOtherMasterService
{
    Task<List<MasterCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<OtherMasterListDto>> GetAllAsync(int? masterId = null, CancellationToken cancellationToken = default);
    Task<List<OtherMasterLookupDto>> GetLookupByMasterNameAsync(string masterName, CancellationToken cancellationToken = default);
    Task<OtherMasterListDto?> GetByIdAsync(int masterTypeId, CancellationToken cancellationToken = default);
    Task<OtherMasterListDto> CreateAsync(OtherMasterCreateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<OtherMasterListDto?> UpdateAsync(int masterTypeId, OtherMasterUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int masterTypeId, string? userId, CancellationToken cancellationToken = default);
}
