using System.Security.Cryptography;

namespace MiniB2B.Web.Services;

/// <summary>
/// PBKDF2 (RFC2898) tabanlı şifre hashleme. Şifreler asla açık metin
/// olarak saklanmaz; her kullanıcı için rastgele bir salt üretilir.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;       // 128 bit
    private const int HashSize = 32;       // 256 bit
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return (hash, salt);
    }

    public bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return CryptographicOperations.FixedTimeEquals(computedHash, hash);
    }
}
