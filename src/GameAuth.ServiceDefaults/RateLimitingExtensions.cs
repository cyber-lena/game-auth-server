using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GameAuth.ServiceDefaults;

/// <summary>
/// Configures IP-based rate limiting using AspNetCoreRateLimit with a Redis
/// distributed store so limits are enforced consistently across all replicas.
/// Rules are read from the "IpRateLimiting" configuration section; sensible
/// defaults apply when the section is absent. Requires an
/// <see cref="IConnectionMultiplexer"/> to be registered by the host.
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // In-memory cache is still used for policy/config storage.
        services.AddMemoryCache();

        var section = configuration.GetSection("IpRateLimiting");
        if (section.Exists())
        {
            services.Configure<IpRateLimitOptions>(section);
        }
        else
        {
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.RealIpHeader = "X-Real-IP";
                options.ClientIdHeader = "X-ClientId";
                options.GeneralRules = new List<RateLimitRule>
                {
                    new() { Endpoint = "*", Period = "1s", Limit = 20 },
                    new() { Endpoint = "*", Period = "1m", Limit = 300 }
                };
            });
        }

        var policiesSection = configuration.GetSection("IpRateLimitPolicies");
        if (policiesSection.Exists())
        {
            services.Configure<IpRateLimitPolicies>(policiesSection);
        }

        // Distributed counter store backed by Redis.
        services.AddRedisRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    public static WebApplication UseRateLimitingDefaults(this WebApplication app)
    {
        app.UseIpRateLimiting();
        return app;
    }
}
