using GameAuth.Shared.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace GameAuth.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serialized, expiration);
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _database.KeyExistsAsync(key);
    }
}

public class RedisConnectionFactory
{
    private static IConnectionMultiplexer? _connection;
    private static readonly object _lock = new();

    public static IConnectionMultiplexer GetConnection(string connectionString)
    {
        if (_connection == null || !_connection.IsConnected)
        {
            lock (_lock)
            {
                if (_connection == null || !_connection.IsConnected)
                {
                    _connection = ConnectionMultiplexer.Connect(connectionString);
                }
            }
        }

        return _connection;
    }
}
