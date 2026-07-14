using GameAuth.Infrastructure.Persistence;
using GameAuth.Infrastructure.Persistence.Entities;
using GameAuth.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameAuth.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<GameAuthDbContext> _options;

    public UserRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<GameAuthDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new GameAuthDbContext(_options);
        context.Database.EnsureCreated();
    }

    private GameAuthDbContext CreateContext() => new(_options);

    [Fact]
    public async Task AddAsync_And_GetByUsername_RoundTrips()
    {
        await using (var context = CreateContext())
        {
            var repo = new UserRepository(context);
            await repo.AddAsync(new User { Username = "alice", Email = "alice@example.com" });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var repo = new UserRepository(context);
            var user = await repo.GetByUsernameAsync("alice");

            Assert.NotNull(user);
            Assert.Equal("alice@example.com", user!.Email);
        }
    }

    [Fact]
    public async Task UsernameExists_ReturnsExpected()
    {
        await using (var context = CreateContext())
        {
            var repo = new UserRepository(context);
            await repo.AddAsync(new User { Username = "bob", Email = "bob@example.com" });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var repo = new UserRepository(context);
            Assert.True(await repo.UsernameExistsAsync("bob"));
            Assert.False(await repo.UsernameExistsAsync("nobody"));
        }
    }

    [Fact]
    public async Task GetUserWithCredential_IncludesCredential()
    {
        await using (var context = CreateContext())
        {
            var repo = new UserRepository(context);
            var user = new User { Username = "carol", Email = "carol@example.com" };
            user.Credential = new Credential { PasswordHash = "hash" };
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var repo = new UserRepository(context);
            var stored = await repo.GetByUsernameAsync("carol");
            var withCred = await repo.GetUserWithCredentialAsync(stored!.Id);

            Assert.NotNull(withCred!.Credential);
            Assert.Equal("hash", withCred.Credential!.PasswordHash);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
