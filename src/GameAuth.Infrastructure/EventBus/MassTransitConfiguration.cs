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

                // Configure endpoints for consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
