using CLMS_APIs.Models.Entities;

namespace CLMS_APIs.Services;

public interface ITokenService
{
    string GenerateToken(LoginEntity user);
}
