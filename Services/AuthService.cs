using CLMS_APIs.Data;
using CLMS_APIs.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Services;

public class AuthService : IAuthService
{
    private readonly ClmsDbContext _context;
    private readonly IPasswordVerifier _passwordVerifier;
    private readonly ITokenService _tokenService;

    public AuthService(
        ClmsDbContext context,
        IPasswordVerifier passwordVerifier,
        ITokenService tokenService)
    {
        _context = context;
        _passwordVerifier = passwordVerifier;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Safe, parameterized LINQ query against the Login table via EF Core
        var user = await _context.Logins
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Isolated password verification
        var isPasswordValid = _passwordVerifier.VerifyPassword(request.Password, user.Password);
        if (!isPasswordValid)
        {
            return null;
        }

        // Update Mod_dt to DateTime.Now
        user.ModDt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var token = _tokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Uid = user.Uid,
            Username = user.Username,
            Role = user.Role,
            EmployeeName = user.EmployeeName,
            ContractorId = user.ContractorId,
            Email = user.Email,
            CompLogo = user.CompLogo,
            Token = token
        };
    }
}
