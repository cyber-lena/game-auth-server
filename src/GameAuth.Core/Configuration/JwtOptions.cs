namespace GameAuth.Core.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "GameAuth";
    public string Audience { get; set; } = "GameAuth.Clients";

    /// <summary>Signing algorithm: "RS256" (asymmetric, recommended) or "HS256" (symmetric).</summary>
    public string Algorithm { get; set; } = "RS256";

    /// <summary>Symmetric signing key. Only used when <see cref="Algorithm"/> is HS256.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Path to the RSA private key in PEM (PKCS#8) format. Required for signing when using RS256.</summary>
    public string PrivateKeyPath { get; set; } = string.Empty;

    /// <summary>Path to the RSA public key in PEM (SPKI) format. Used for validation when using RS256.</summary>
    public string PublicKeyPath { get; set; } = string.Empty;

    /// <summary>Inline RSA private key PEM contents. Takes precedence over <see cref="PrivateKeyPath"/> when set.</summary>
    public string PrivateKeyPem { get; set; } = string.Empty;

    /// <summary>Inline RSA public key PEM contents. Takes precedence over <see cref="PublicKeyPath"/> when set.</summary>
    public string PublicKeyPem { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
