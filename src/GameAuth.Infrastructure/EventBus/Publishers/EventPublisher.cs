using GameAuth.Shared.Interfaces;
using MassTransit;

namespace GameAuth.Infrastructure.EventBus.Publishers;

public class EventPublisher : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}
