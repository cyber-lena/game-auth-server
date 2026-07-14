using GameAuth.AuditService.Storage;
using GameAuth.Infrastructure.Persistence.Entities;
using GameAuth.Protos.Audit;
using Grpc.Core;

namespace GameAuth.AuditService.Services;

public class AuditGrpcService : Protos.Audit.AuditService.AuditServiceBase
{
    private readonly IAuditLogStore _store;
    private readonly ILogger<AuditGrpcService> _logger;

    public AuditGrpcService(IAuditLogStore store, ILogger<AuditGrpcService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public override async Task<LogEventResponse> LogEvent(LogEventRequest request, ServerCallContext context)
    {
        var log = new AuditLog
        {
            UserId = request.UserId > 0 ? request.UserId : null,
            EventType = request.EventType,
            EventSource = request.EventSource,
            IpAddress = string.IsNullOrEmpty(request.IpAddress) ? null : request.IpAddress,
            UserAgent = string.IsNullOrEmpty(request.UserAgent) ? null : request.UserAgent,
            Status = string.IsNullOrEmpty(request.Status) ? null : request.Status,
            Timestamp = DateTime.UtcNow
        };

        var id = await _store.AddAsync(log, context.CancellationToken);
        return new LogEventResponse { Success = true, Message = "Logged", EventId = id };
    }

    public override async Task<QueryLogsResponse> QueryLogs(QueryLogsRequest request, ServerCallContext context)
    {
        var from = request.FromTimestampSeconds > 0
            ? DateTimeOffset.FromUnixTimeSeconds(request.FromTimestampSeconds).UtcDateTime
            : (DateTime?)null;
        var to = request.ToTimestampSeconds > 0
            ? DateTimeOffset.FromUnixTimeSeconds(request.ToTimestampSeconds).UtcDateTime
            : (DateTime?)null;

        var (logs, total) = await _store.QueryAsync(
            request.UserId,
            request.EventType,
            from,
            to,
            request.PageNumber,
            request.PageSize,
            context.CancellationToken);

        var response = new QueryLogsResponse { Success = true, TotalCount = total };
        response.Logs.AddRange(logs.Select(l => new AuditLogEntry
        {
            Id = l.Id,
            UserId = l.UserId ?? 0,
            EventType = l.EventType,
            EventSource = l.EventSource,
            IpAddress = l.IpAddress ?? string.Empty,
            UserAgent = l.UserAgent ?? string.Empty,
            Status = l.Status ?? string.Empty,
            TimestampSeconds = new DateTimeOffset(l.Timestamp, TimeSpan.Zero).ToUnixTimeSeconds()
        }));

        return response;
    }

    public override async Task<SecurityEventsResponse> GetSecurityEvents(SecurityEventsRequest request, ServerCallContext context)
    {
        var from = request.FromTimestampSeconds > 0
            ? DateTimeOffset.FromUnixTimeSeconds(request.FromTimestampSeconds).UtcDateTime
            : (DateTime?)null;
        var to = request.ToTimestampSeconds > 0
            ? DateTimeOffset.FromUnixTimeSeconds(request.ToTimestampSeconds).UtcDateTime
            : (DateTime?)null;

        var (logs, total) = await _store.QueryAsync(
            null,
            "SecurityEvent",
            from,
            to,
            request.PageNumber,
            request.PageSize,
            context.CancellationToken);

        var response = new SecurityEventsResponse { Success = true, TotalCount = total };
        response.Events.AddRange(logs.Select(l => new SecurityEventEntry
        {
            Id = l.Id,
            EventType = l.EventType,
            Severity = l.Status ?? "info",
            UserId = l.UserId ?? 0,
            IpAddress = l.IpAddress ?? string.Empty,
            Description = l.EventSource,
            TimestampSeconds = new DateTimeOffset(l.Timestamp, TimeSpan.Zero).ToUnixTimeSeconds()
        }));

        return response;
    }
}
