using GameAuth.AuditService.Storage;
using GameAuth.Infrastructure.Persistence.Entities;
using GameAuth.Shared.Events;
using MassTransit;

namespace GameAuth.AuditService.Consumers;

public class UserLoggedInAuditConsumer : IConsumer<UserLoggedInEvent>
{
    private readonly IAuditLogStore _store;

    public UserLoggedInAuditConsumer(IAuditLogStore store)
    {
        _store = store;
    }

    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var message = context.Message;
        await _store.AddAsync(new AuditLog
        {
            UserId = message.UserId,
            EventType = "UserLoggedIn",
            EventSource = "GameAuth.Core",
            IpAddress = message.IpAddress,
            UserAgent = message.UserAgent,
            Status = "success",
            Timestamp = message.Timestamp
        }, context.CancellationToken);
    }
}

public class UserRegisteredAuditConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IAuditLogStore _store;

    public UserRegisteredAuditConsumer(IAuditLogStore store)
    {
        _store = store;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        await _store.AddAsync(new AuditLog
        {
            UserId = message.UserId,
            EventType = "UserRegistered",
            EventSource = "GameAuth.Core",
            IpAddress = message.IpAddress,
            Status = "success",
            Timestamp = message.Timestamp
        }, context.CancellationToken);
    }
}

public class SecurityEventAuditConsumer : IConsumer<SecurityEventTriggeredEvent>
{
    private readonly IAuditLogStore _store;

    public SecurityEventAuditConsumer(IAuditLogStore store)
    {
        _store = store;
    }

    public async Task Consume(ConsumeContext<SecurityEventTriggeredEvent> context)
    {
        var message = context.Message;
        await _store.AddAsync(new AuditLog
        {
            UserId = message.UserId,
            EventType = "SecurityEvent",
            EventSource = message.Description,
            IpAddress = message.IpAddress,
            Status = message.Severity,
            Timestamp = message.Timestamp
        }, context.CancellationToken);
    }
}
