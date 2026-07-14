using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameAuth.Infrastructure.EventBus;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            // Allow services to register their consumers
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var portStr = configuration["RabbitMQ:Port"];
                var port = !string.IsNullOrEmpty(portStr) && int.TryParse(portStr, out var p) ? p : 5672;
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, (ushort)port, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Bus-level circuit breaker: stops delivery to failing consumers
                // when the failure rate spikes, giving downstream systems time to recover.
                cfg.UseKillSwitch(options => options
                    .SetActivationThreshold(10)
                    .SetTripThreshold(0.5)
                    .SetRestartTimeout(TimeSpan.FromMinutes(1)));

                // In-process immediate retries with exponential backoff for transient faults.
                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromMilliseconds(500),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(2)));

                // Delayed redelivery for longer outages (moves the message back to the queue later).
                cfg.UseDelayedRedelivery(r => r.Intervals(
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(2),
                    TimeSpan.FromMinutes(5)));

                // Configure endpoints for consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
