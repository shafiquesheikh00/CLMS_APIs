namespace CLMS_APIs.Models.DTOs;

public class LoginResponseDto
{
    public string Uid { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? EmployeeName { get; set; }
    public int? ContractorId { get; set; }
    public string? Email { get; set; }
    public string? CompLogo { get; set; }
    public string Token { get; set; } = string.Empty;
}
