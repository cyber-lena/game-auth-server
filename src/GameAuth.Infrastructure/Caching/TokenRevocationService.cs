using StackExchange.Redis;

namespace GameAuth.Infrastructure.Caching;

public interface ITokenRevocationService
{
    Task RevokeTokenAsync(string tokenId, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(long userId, CancellationToken cancellationToken = default);
}

public class TokenRevocationService : ITokenRevocationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private const string RevokedTokenPrefix = "revoked:token:";
    private const string RevokedUserPrefix = "revoked:user:";

    public TokenRevocationService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task RevokeTokenAsync(string tokenId, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var key = $"{RevokedTokenPrefix}{tokenId}";
        await _database.StringSetAsync(key, "revoked", expiration);
    }

    public async Task<bool> IsTokenRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        var key = $"{RevokedTokenPrefix}{tokenId}";
        return await _database.KeyExistsAsync(key);
    }

    public async Task RevokeAllUserTokensAsync(long userId, CancellationToken cancellationToken = default)
    {
        var key = $"{RevokedUserPrefix}{userId}";
        // Set with a long expiration (e.g., refresh token lifetime)
        await _database.StringSetAsync(key, DateTime.UtcNow.Ticks.ToString(), TimeSpan.FromDays(7));
    }
}
