namespace GameAuth.Core.Security.ExternalIdentity;

/// <summary>
/// Verifies an external identity provider token (e.g. an OIDC ID token) server-side.
/// Implementations are resolved by their <see cref="Provider"/> key.
/// </summary>
public interface IExternalIdentityVerifier
{
    /// <summary>Provider key this verifier handles (case-insensitive), e.g. "google".</summary>
    string Provider { get; }

    /// <summary>
    /// Validates the supplied token and returns the verified identity, or null when the
    /// token is invalid, expired, or issued for a different audience.
    /// </summary>
    Task<ExternalIdentityResult?> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}
