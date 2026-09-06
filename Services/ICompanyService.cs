using CLMS_APIs.Models.DTOs;

namespace CLMS_APIs.Services;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllCompaniesAsync(CancellationToken cancellationToken = default);
    Task<CompanyDetailDto?> GetCompanyByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CompanyDetailDto> CreateCompanyAsync(CompanyCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<CompanyDetailDto?> UpdateCompanyAsync(string id, CompanyCreateUpdateDto dto, string? userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteCompanyAsync(string id, string? userId, CancellationToken cancellationToken = default);
}
