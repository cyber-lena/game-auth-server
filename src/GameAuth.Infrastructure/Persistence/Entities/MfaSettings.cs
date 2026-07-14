namespace GameAuth.Infrastructure.Persistence.Entities;

public class MfaSettings
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? MfaType { get; set; }
    public string? MfaSecret { get; set; }
    public string[]? BackupCodes { get; set; }
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}
