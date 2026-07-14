using GameAuth.Infrastructure.Caching;
using GameAuth.Infrastructure.EventBus;
using GameAuth.Infrastructure.EventBus.Publishers;
using GameAuth.Infrastructure.Persistence;
using GameAuth.Infrastructure.Persistence.Repositories;
using GameAuth.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GameAuth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MassTransit.IBusRegistrationConfigurator>? configureMassTransitConsumers = null)
    {
        // Database
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");

        services.AddDbContext<GameAuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register DbContext as DbContext for UnitOfWork
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<GameAuthDbContext>());

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            RedisConnectionFactory.GetConnection(redisConnectionString));

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<ISessionCacheService, SessionCacheService>();
        services.AddSingleton<ITokenRevocationService, TokenRevocationService>();

        // MassTransit & Event Bus
        services.AddMassTransitWithRabbitMq(configuration, configureMassTransitConsumers);
        services.AddScoped<IEventBus, EventPublisher>();

        return services;
    }
}
