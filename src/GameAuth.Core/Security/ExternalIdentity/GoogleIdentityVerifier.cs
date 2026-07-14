using GameAuth.Core.Configuration;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace GameAuth.Core.Security.ExternalIdentity;

/// <summary>
/// Verifies Google ID tokens using <see cref="GoogleJsonWebSignature"/>, ensuring the token
/// was issued for the configured client ID (audience) and is correctly signed by Google.
/// </summary>
public sealed class GoogleIdentityVerifier : IExternalIdentityVerifier
{
    public const string ProviderKey = "google";

    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleIdentityVerifier> _logger;

    public GoogleIdentityVerifier(IOptions<ExternalAuthOptions> options, ILogger<GoogleIdentityVerifier> logger)
    {
        _options = options.Value.Google;
        _logger = logger;
    }

    public string Provider => ProviderKey;

    public async Task<ExternalIdentityResult?> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Google external login attempted but the provider is disabled.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var settings = new GoogleJsonWebSignature.ValidationSettings();
        if (!string.IsNullOrWhiteSpace(_options.ClientId))
        {
            settings.Audience = new[] { _options.ClientId };
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new ExternalIdentityResult(
                Provider: ProviderKey,
                ProviderUserId: payload.Subject,
                Email: payload.Email,
                EmailVerified: payload.EmailVerified,
                Name: payload.Name);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed.");
            return null;
        }
    }
}
