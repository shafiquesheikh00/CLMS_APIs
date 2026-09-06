using CLMS_APIs.Models.DTOs;
using CLMS_APIs.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CLMS_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowReactApp")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService _authService, ILogger<AuthController> logger)
    {
        this._authService = _authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a CLMS user and returns user info with a JWT token.
    /// </summary>
    /// <param name="request">User credentials (Username and Password).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>LoginResponseDto with token if authenticated; otherwise 400, 401, or 500 error.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and Password are required." });
        }

        try
        {
            var response = await _authService.AuthenticateAsync(request, cancellationToken);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid Username or Password" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during authentication for username: {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while processing your request."
            });
        }
    }
}
