using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;

namespace GameAuth.ServiceDefaults;

/// <summary>
/// Provides shared Serilog structured logging configuration for all services.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog as the host logger using settings from configuration,
    /// enriched with trace/span context for correlation with OpenTelemetry.
    /// </summary>
    public static IHostBuilder UseSerilogDefaults(this IHostBuilder hostBuilder, string serviceName)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .Enrich.WithProperty("ServiceName", serviceName);
        });
    }
}
