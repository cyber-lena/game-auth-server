using System.Text.Json;
using StackExchange.Redis;

namespace GameAuth.ProfileService.Storage;

public record UserProfile
{
    public required long UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
    public Dictionary<string, string> Metadata { get; init; } = new();
    public Dictionary<string, string> Settings { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public interface IProfileStore
{
    Task<UserProfile?> GetAsync(long userId);
    Task SaveAsync(UserProfile profile);
}

public class RedisProfileStore : IProfileStore
{
    private readonly IDatabase _database;
    private const string ProfilePrefix = "profile:";

    public RedisProfileStore(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<UserProfile?> GetAsync(long userId)
    {
        var value = await _database.StringGetAsync(Key(userId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<UserProfile>((string)value!);
    }

    public async Task SaveAsync(UserProfile profile)
    {
        await _database.StringSetAsync(Key(profile.UserId), JsonSerializer.Serialize(profile));
    }

    private static string Key(long userId) => $"{ProfilePrefix}{userId}";
}
