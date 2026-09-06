namespace CLMS_APIs.Services;

/// <summary>
/// Isolates password verification logic to allow swapping legacy plaintext comparison
/// with BCrypt, PBKDF2, or ASP.NET Core Identity PasswordHasher without altering controllers or services.
/// </summary>
public interface IPasswordVerifier
{
    bool VerifyPassword(string providedPassword, string storedPassword);
}
