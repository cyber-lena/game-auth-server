using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace GameAuth.ServiceDefaults.Middleware;

/// <summary>
/// Ensures every request has a correlation id (read from the incoming
/// <c>X-Correlation-ID</c> header or generated), echoes it on the response,
/// and pushes it into the Serilog log context for cross-service correlation.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            && !string.IsNullOrWhiteSpace(value))
        {
            return value!;
        }

        return Guid.NewGuid().ToString("N");
    }
}
