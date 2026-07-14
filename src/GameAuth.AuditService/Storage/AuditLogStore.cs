using GameAuth.Infrastructure.Persistence;
using GameAuth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameAuth.AuditService.Storage;

public interface IAuditLogStore
{
    Task<long> AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuditLog> Logs, int TotalCount)> QueryAsync(
        long? userId,
        string? eventType,
        DateTime? from,
        DateTime? to,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public class AuditLogStore : IAuditLogStore
{
    private readonly GameAuthDbContext _db;

    public AuditLogStore(GameAuthDbContext db)
    {
        _db = db;
    }

    public async Task<long> AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        return log.Id;
    }

    public async Task<(IReadOnlyList<AuditLog> Logs, int TotalCount)> QueryAsync(
        long? userId,
        string? eventType,
        DateTime? from,
        DateTime? to,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (userId is > 0)
        {
            query = query.Where(l => l.UserId == userId);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(l => l.EventType == eventType);
        }

        if (from is not null)
        {
            query = query.Where(l => l.Timestamp >= from);
        }

        if (to is not null)
        {
            query = query.Where(l => l.Timestamp <= to);
        }

        var total = await query.CountAsync(cancellationToken);

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (logs, total);
    }
}
