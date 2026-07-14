namespace GameAuth.Core.Security.ExternalIdentity;

/// <summary>
/// Resolves the appropriate <see cref="IExternalIdentityVerifier"/> for a given provider key.
/// Registered as a singleton; new providers are added simply by registering another verifier.
/// </summary>
public sealed class ExternalIdentityVerifierRegistry
{
    private readonly IReadOnlyDictionary<string, IExternalIdentityVerifier> _verifiers;

    public ExternalIdentityVerifierRegistry(IEnumerable<IExternalIdentityVerifier> verifiers)
    {
        _verifiers = verifiers.ToDictionary(v => v.Provider, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Returns the verifier for the provider, or null when the provider is unsupported.</summary>
    public IExternalIdentityVerifier? Resolve(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return null;
        }

        return _verifiers.TryGetValue(provider, out var verifier) ? verifier : null;
    }
}
