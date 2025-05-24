using System.Security.Cryptography;
using System.Text;

namespace TaskFlow.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hashToVerify = HashPassword(password);
        return hashToVerify.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
    }

    public string GenerateApiToken()
    {
        return Guid.NewGuid().ToString();
    }
}