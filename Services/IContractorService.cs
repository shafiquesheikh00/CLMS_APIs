using CLMS_APIs.Models.DTOs;

namespace CLMS_APIs.Services;

public interface IContractorService
{
    Task<List<ContractorListDto>> GetContractorsAsync(bool? activeOnly, CancellationToken cancellationToken = default);
    Task<ContractorDetailDto?> GetContractorByIdAsync(decimal id, CancellationToken cancellationToken = default);
    Task<List<ContractorTypeLookupDto>> GetContractorTypesAsync(CancellationToken cancellationToken = default);
    Task<ContractorDetailDto> CreateContractorAsync(ContractorCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<ContractorDetailDto?> UpdateContractorAsync(decimal id, ContractorCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteContractorAsync(decimal id, string? userId, CancellationToken cancellationToken = default);
}
