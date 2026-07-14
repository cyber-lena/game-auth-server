namespace GameAuth.Shared.Events;

public interface IEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    string CorrelationId { get; }
}

public abstract record BaseEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string CorrelationId { get; init; }
}

public record UserRegisteredEvent : BaseEvent
{
    public required long UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? IpAddress { get; init; }
}

public record UserLoggedInEvent : BaseEvent
{
    public required long UserId { get; init; }
    public required string Username { get; init; }
    public required string SessionId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public bool MfaUsed { get; init; }
}

public record MfaChallengeInitiatedEvent : BaseEvent
{
    public required long UserId { get; init; }
    public required string SessionId { get; init; }
    public required string MfaType { get; init; }
    public string? IpAddress { get; init; }
}

public record TokenGeneratedEvent : BaseEvent
{
    public required long UserId { get; init; }
    public required string TokenType { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string? SessionId { get; init; }
}

public record UserProfileUpdatedEvent : BaseEvent
{
    public required long UserId { get; init; }
    public required string UpdatedFields { get; init; }
}

public record SecurityEventTriggeredEvent : BaseEvent
{
    public required string EventType { get; init; }
    public required string Severity { get; init; }
    public long? UserId { get; init; }
    public string? IpAddress { get; init; }
    public required string Description { get; init; }
}

public record ServiceHealthEvent : BaseEvent
{
    public required string ServiceName { get; init; }
    public required string HealthStatus { get; init; }
    public Dictionary<string, object> Details { get; init; } = new();
}
