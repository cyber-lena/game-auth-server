using System.Text.Json;
using StackExchange.Redis;

namespace GameAuth.Infrastructure.Caching;

public record SessionState
{
    public required long UserId { get; init; }
    public required string SessionId { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public interface ISessionCacheService
{
    Task<SessionState?> GetSessionAsync(string sessionKey, CancellationToken cancellationToken = default);
    Task SetSessionAsync(string sessionKey, SessionState session, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(string sessionKey, CancellationToken cancellationToken = default);
    Task<bool> SessionExistsAsync(string sessionKey, CancellationToken cancellationToken = default);
}

public class SessionCacheService : ISessionCacheService
{
    private readonly IDatabase _database;
    private const string SessionPrefix = "session:";

    public SessionCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<SessionState?> GetSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(Key(sessionKey));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<SessionState>((string)value!);
    }

    public async Task SetSessionAsync(string sessionKey, SessionState session, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        await _database.StringSetAsync(Key(sessionKey), JsonSerializer.Serialize(session), expiration);
    }

    public async Task RemoveSessionAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(Key(sessionKey));
    }

    public async Task<bool> SessionExistsAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        return await _database.KeyExistsAsync(Key(sessionKey));
    }

    private static string Key(string sessionKey) => $"{SessionPrefix}{sessionKey}";
}
