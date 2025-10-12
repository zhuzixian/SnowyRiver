using System.Security.Cryptography;

namespace SnowyRiver.Accounts.Domain.Helpers;
public static class PasswordHelper
{
    public static string CreateSalt(int size = 16)
    {
        var salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    public static string CreatePassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = $"{password}{salt}";
        var saltedPasswordBytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
        var hash = sha256.ComputeHash(saltedPasswordBytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string salt, string hash)
    {
        var newHash = CreatePassword(password, salt);
        return newHash == hash;
    }
}
