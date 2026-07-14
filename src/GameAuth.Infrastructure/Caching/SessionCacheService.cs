using StackExchange.Redis;

namespace GameAuth.Infrastructure.Caching;

public interface ISessionCacheService
{
    Task<string?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task SetSessionAsync(string sessionId, string userId, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<bool> SessionExistsAsync(string sessionId, CancellationToken cancellationToken = default);
}

public class SessionCacheService : ISessionCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private const string SessionPrefix = "session:";

    public SessionCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<string?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = $"{SessionPrefix}{sessionId}";
        var value = await _database.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : (string)value!;
    }

    public async Task SetSessionAsync(string sessionId, string userId, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var key = $"{SessionPrefix}{sessionId}";
        await _database.StringSetAsync(key, userId, expiration);
    }

    public async Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = $"{SessionPrefix}{sessionId}";
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> SessionExistsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var key = $"{SessionPrefix}{sessionId}";
        return await _database.KeyExistsAsync(key);
    }
}
