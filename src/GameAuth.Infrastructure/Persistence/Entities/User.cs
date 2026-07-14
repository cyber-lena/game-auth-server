namespace GameAuth.Infrastructure.Persistence.Entities;

public class User
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Credential? Credential { get; set; }
    public MfaSettings? MfaSettings { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
}
