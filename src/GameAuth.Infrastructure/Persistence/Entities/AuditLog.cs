namespace GameAuth.Infrastructure.Persistence.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public required string EventType { get; set; }
    public required string EventSource { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Status { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
}
