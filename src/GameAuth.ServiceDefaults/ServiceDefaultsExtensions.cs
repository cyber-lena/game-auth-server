using GameAuth.ServiceDefaults.Middleware;
using GameAuth.ServiceDefaults.Resilience;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameAuth.ServiceDefaults;

/// <summary>
/// Aggregates cross-cutting host defaults (telemetry, health checks, and the
/// standard middleware pipeline) shared by every GameAuth service.
/// </summary>
public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// Registers shared services: OpenTelemetry tracing/metrics and health checks.
    /// Serilog is configured separately via <see cref="SerilogExtensions.UseSerilogDefaults"/>
    /// on the host builder.
    /// </summary>
    public static IServiceCollection AddServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        services.AddOpenTelemetryDefaults(configuration, serviceName);
        services.AddHealthChecks();
        services.AddRateLimitingDefaults(configuration);
        services.AddResilienceDefaults(configuration);

        return services;
    }

    /// <summary>
    /// Wires the standard middleware pipeline (correlation id, global exception
    /// handling), Prometheus metrics endpoint, and health endpoints.
    /// Should be called early in the pipeline, before endpoint routing.
    /// </summary>
    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        app.UseRateLimitingDefaults();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapOpenTelemetryDefaults();
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");
        app.MapHealthChecks("/health/live");

        return app;
    }
}
