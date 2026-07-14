using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GameAuth.ServiceDefaults;

/// <summary>
/// Provides shared OpenTelemetry tracing and metrics configuration for all services.
/// </summary>
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
            .AddTelemetrySdk();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }

    /// <summary>
    /// Maps the Prometheus scraping endpoint (default: /metrics).
    /// </summary>
    public static WebApplication MapOpenTelemetryDefaults(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
        return app;
    }
}
