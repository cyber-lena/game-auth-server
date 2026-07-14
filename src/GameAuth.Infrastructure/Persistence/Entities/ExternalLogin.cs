namespace GameAuth.Infrastructure.Persistence.Entities;

public class ExternalLogin
{
    public long Id { get; set; }
    public long UserId { get; set; }

    /// <summary>Identity provider key (e.g. "google").</summary>
    public required string Provider { get; set; }

    /// <summary>The provider's stable unique identifier for the user (e.g. Google "sub" claim).</summary>
    public required string ProviderUserId { get; set; }

    /// <summary>Email reported by the provider at link time.</summary>
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}
