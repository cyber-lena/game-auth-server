using System.Security.Cryptography;
using System.Text;
using GameAuth.Core.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GameAuth.Core.Security;

/// <summary>
/// Resolves signing and validation keys for JWTs based on <see cref="JwtOptions"/>.
/// Supports asymmetric RS256 (RSA public/private keys in PEM format) and symmetric HS256.
/// The RSA instance is owned by this provider and kept alive for the lifetime of the application.
/// </summary>
public sealed class JwtKeyProvider : IDisposable
{
    private readonly RSA? _rsa;

    public JwtKeyProvider(JwtOptions options)
    {
        if (IsRs256(options.Algorithm))
        {
            _rsa = RSA.Create();
            var privatePem = ReadPem(options.PrivateKeyPem, options.PrivateKeyPath);
            var publicPem = ReadPem(options.PublicKeyPem, options.PublicKeyPath);

            if (!string.IsNullOrWhiteSpace(privatePem))
            {
                _rsa.ImportFromPem(privatePem);
            }
            else if (!string.IsNullOrWhiteSpace(publicPem))
            {
                _rsa.ImportFromPem(publicPem);
            }
            else
            {
                throw new InvalidOperationException(
                    "RS256 is configured but no RSA key was provided. Set Jwt:PrivateKeyPath/PublicKeyPath or the inline PEM variants.");
            }

            var rsaKey = new RsaSecurityKey(_rsa);
            SigningCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
            ValidationKey = rsaKey;
            Algorithm = SecurityAlgorithms.RsaSha256;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "HS256 is configured but Jwt:SigningKey is missing or shorter than 32 characters.");
            }

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
            SigningCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            ValidationKey = symmetricKey;
            Algorithm = SecurityAlgorithms.HmacSha256;
        }
    }

    /// <summary>Credentials used to sign newly issued tokens.</summary>
    public SigningCredentials SigningCredentials { get; }

    /// <summary>Key used to validate incoming tokens (public key for RS256, shared key for HS256).</summary>
    public SecurityKey ValidationKey { get; }

    /// <summary>The resolved algorithm identifier (e.g. RS256 / HS256).</summary>
    public string Algorithm { get; }

    private static bool IsRs256(string? algorithm) =>
        string.IsNullOrWhiteSpace(algorithm) ||
        algorithm.Equals("RS256", StringComparison.OrdinalIgnoreCase);

    private static string ReadPem(string inlinePem, string path)
    {
        if (!string.IsNullOrWhiteSpace(inlinePem))
        {
            return inlinePem;
        }

        return !string.IsNullOrWhiteSpace(path) && File.Exists(path)
            ? File.ReadAllText(path)
            : string.Empty;
    }

    public void Dispose() => _rsa?.Dispose();
}
