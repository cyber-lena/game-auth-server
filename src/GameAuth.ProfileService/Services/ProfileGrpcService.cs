using GameAuth.ProfileService.Storage;
using GameAuth.Protos.Common;
using GameAuth.Protos.Profile;
using GameAuth.Shared.Events;
using GameAuth.Shared.Interfaces;
using Grpc.Core;

namespace GameAuth.ProfileService.Services;

public class ProfileGrpcService : Protos.Profile.ProfileService.ProfileServiceBase
{
    private readonly IProfileStore _store;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProfileGrpcService> _logger;

    public ProfileGrpcService(IProfileStore store, IEventBus eventBus, ILogger<ProfileGrpcService> logger)
    {
        _store = store;
        _eventBus = eventBus;
        _logger = logger;
    }

    public override async Task<ProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        var profile = await _store.GetAsync(request.UserId);
        if (profile is null)
        {
            return new ProfileResponse { Success = false };
        }

        var response = new ProfileResponse
        {
            Success = true,
            User = new UserIdentity { UserId = profile.UserId, Username = profile.DisplayName },
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            CreatedAtSeconds = new DateTimeOffset(profile.CreatedAt).ToUnixTimeSeconds(),
            UpdatedAtSeconds = new DateTimeOffset(profile.UpdatedAt).ToUnixTimeSeconds()
        };
        response.Metadata.Add(profile.Metadata);
        return response;
    }

    public override async Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
    {
        var existing = await _store.GetAsync(request.UserId) ?? new UserProfile { UserId = request.UserId };

        var updated = existing with
        {
            DisplayName = request.DisplayName,
            AvatarUrl = request.AvatarUrl,
            Metadata = new Dictionary<string, string>(request.Metadata),
            UpdatedAt = DateTime.UtcNow
        };

        await _store.SaveAsync(updated);

        await _eventBus.PublishAsync(new UserProfileUpdatedEvent
        {
            CorrelationId = context.GetHttpContext()?.TraceIdentifier ?? Guid.NewGuid().ToString(),
            UserId = request.UserId,
            UpdatedFields = "DisplayName,AvatarUrl,Metadata"
        }, context.CancellationToken);

        return new UpdateProfileResponse { Success = true, Message = "Profile updated" };
    }

    public override async Task<SettingsResponse> GetSettings(GetSettingsRequest request, ServerCallContext context)
    {
        var profile = await _store.GetAsync(request.UserId);
        var response = new SettingsResponse { Success = profile is not null };
        if (profile is not null)
        {
            response.Settings.Add(profile.Settings);
        }
        return response;
    }

    public override async Task<UpdateSettingsResponse> UpdateSettings(UpdateSettingsRequest request, ServerCallContext context)
    {
        var existing = await _store.GetAsync(request.UserId) ?? new UserProfile { UserId = request.UserId };

        var updated = existing with
        {
            Settings = new Dictionary<string, string>(request.Settings),
            UpdatedAt = DateTime.UtcNow
        };

        await _store.SaveAsync(updated);
        return new UpdateSettingsResponse { Success = true, Message = "Settings updated" };
    }
}
