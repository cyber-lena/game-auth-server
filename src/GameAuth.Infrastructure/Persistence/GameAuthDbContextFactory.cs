using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GameAuth.Infrastructure.Persistence;

public class GameAuthDbContextFactory : IDesignTimeDbContextFactory<GameAuthDbContext>
{
    public GameAuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameAuthDbContext>();

        // For design-time, use a connection string from environment or default
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL")
            ?? "Host=localhost;Database=gameauth;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new GameAuthDbContext(optionsBuilder.Options);
    }
}
