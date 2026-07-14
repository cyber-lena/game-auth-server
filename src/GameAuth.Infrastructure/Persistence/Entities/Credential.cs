namespace GameAuth.Infrastructure.Persistence.Entities;

public class Credential
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}
