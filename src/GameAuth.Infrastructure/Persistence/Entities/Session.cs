namespace GameAuth.Infrastructure.Persistence.Entities;

public class Session
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public required string SessionId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}
