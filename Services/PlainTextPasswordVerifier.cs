namespace CLMS_APIs.Services;

/// <summary>
/// Legacy plaintext password verifier.
/// Compares the provided password against the existing plaintext password stored in the database.
/// To switch to BCrypt or Argon2/Identity, implement IPasswordVerifier and update DI registration in Program.cs.
/// </summary>
public class PlainTextPasswordVerifier : IPasswordVerifier
{
    public bool VerifyPassword(string providedPassword, string storedPassword)
    {
        if (string.IsNullOrEmpty(providedPassword) || string.IsNullOrEmpty(storedPassword))
        {
            return false;
        }

        return string.Equals(providedPassword, storedPassword, StringComparison.Ordinal);
    }
}
