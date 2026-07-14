using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace GameAuth.Core.Security;

/// <summary>
/// Argon2id-based password hashing and verification.
/// Hash format: {iterations}.{memoryKb}.{parallelism}.{base64Salt}.{base64Hash}
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string encodedHash);
}

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemoryKb = 65536;
    private const int Parallelism = 2;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, salt, Iterations, MemoryKb, Parallelism, HashSize);

        return string.Join('.',
            Iterations,
            MemoryKb,
            Parallelism,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string encodedHash)
    {
        var parts = encodedHash.Split('.');
        if (parts.Length != 5)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var iterations) ||
            !int.TryParse(parts[1], out var memoryKb) ||
            !int.TryParse(parts[2], out var parallelism))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[3]);
        var expected = Convert.FromBase64String(parts[4]);
        var actual = ComputeHash(password, salt, iterations, memoryKb, parallelism, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] ComputeHash(
        string password,
        byte[] salt,
        int iterations,
        int memoryKb,
        int parallelism,
        int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = iterations,
            MemorySize = memoryKb,
            DegreeOfParallelism = parallelism
        };

        return argon2.GetBytes(hashSize);
    }
}
