using CLMS_APIs.Models.DTOs;

namespace CLMS_APIs.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
