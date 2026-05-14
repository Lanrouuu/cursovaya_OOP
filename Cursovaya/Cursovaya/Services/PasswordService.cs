using System.Security.Cryptography;
using System.Text;

namespace Cursovaya.Services;

public class PasswordService
{
    public static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        var currentHash = HashPassword(password);
        return currentHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase);
    }
}
