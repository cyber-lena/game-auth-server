using System.Text.Json;
using GameAuth.ProfileService.Storage;

namespace GameAuth.ProfileService.Tests;

public class UserProfileTests
{
    [Fact]
    public void UserProfile_RoundTrips_ThroughJson()
    {
        var profile = new UserProfile
        {
            UserId = 42,
            DisplayName = "PlayerOne",
            AvatarUrl = "https://cdn.example.com/a.png",
            Metadata = new Dictionary<string, string> { ["country"] = "SE" },
            Settings = new Dictionary<string, string> { ["theme"] = "dark" }
        };

        var json = JsonSerializer.Serialize(profile);
        var deserialized = JsonSerializer.Deserialize<UserProfile>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(42, deserialized!.UserId);
        Assert.Equal("PlayerOne", deserialized.DisplayName);
        Assert.Equal("SE", deserialized.Metadata["country"]);
        Assert.Equal("dark", deserialized.Settings["theme"]);
    }

    [Fact]
    public void UserProfile_WithExpression_UpdatesFields()
    {
        var original = new UserProfile { UserId = 1, DisplayName = "Old" };

        var updated = original with { DisplayName = "New" };

        Assert.Equal("Old", original.DisplayName);
        Assert.Equal("New", updated.DisplayName);
        Assert.Equal(1, updated.UserId);
    }
}
