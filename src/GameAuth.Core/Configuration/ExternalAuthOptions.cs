namespace GameAuth.Core.Configuration;

public class ExternalAuthOptions
{
    public const string SectionName = "ExternalAuth";

    public GoogleAuthOptions Google { get; set; } = new();
}

public class GoogleAuthOptions
{
    /// <summary>Whether Google external login is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>The Google OAuth client ID that ID tokens must be issued for (token audience).</summary>
    public string ClientId { get; set; } = string.Empty;
}
