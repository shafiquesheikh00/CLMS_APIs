using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CLMS_APIs.Models.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CLMS_APIs.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(LoginEntity user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] 
                     ?? jwtSettings["Key"] 
                     ?? throw new InvalidOperationException("JWT secret key is not configured in JwtSettings.");

        var issuer = jwtSettings["Issuer"] ?? "CLMS_API";
        var audience = jwtSettings["Audience"] ?? "CLMS_Client";

        if (!double.TryParse(jwtSettings["ExpiryHours"], out var expiryHours) || expiryHours <= 0)
        {
            expiryHours = 8; // Default 8 hours
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Uid),
            new("uid", user.Uid),
            new(ClaimTypes.Name, user.Username),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
            claims.Add(new Claim("role", user.Role));
        }

        if (user.ContractorId.HasValue)
        {
            claims.Add(new Claim("contractorId", user.ContractorId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim("email", user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.EmployeeName))
        {
            claims.Add(new Claim("employeeName", user.EmployeeName));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expiryHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
