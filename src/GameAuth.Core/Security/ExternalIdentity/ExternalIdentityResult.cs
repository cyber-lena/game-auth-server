namespace GameAuth.Core.Security.ExternalIdentity;

/// <summary>
/// The verified identity returned by an external identity provider after a token has
/// been validated server-side.
/// </summary>
public sealed record ExternalIdentityResult(
    string Provider,
    string ProviderUserId,
    string? Email,
    bool EmailVerified,
    string? Name);
