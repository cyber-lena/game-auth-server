using GameAuth.ProfileService.Storage;
using GameAuth.Shared.Events;
using MassTransit;

namespace GameAuth.ProfileService.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IProfileStore _store;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IProfileStore store, ILogger<UserRegisteredConsumer> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Provisioning profile for user {UserId}", message.UserId);

        var profile = new UserProfile
        {
            UserId = message.UserId,
            DisplayName = message.Username
        };

        await _store.SaveAsync(profile);
    }
}
