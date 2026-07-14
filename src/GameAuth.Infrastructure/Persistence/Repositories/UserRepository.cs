using GameAuth.Infrastructure.Persistence.Entities;
using GameAuth.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameAuth.Infrastructure.Persistence.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithCredentialAsync(long userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithMfaSettingsAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly GameAuthDbContext _dbContext;

    public UserRepository(GameAuthDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetUserWithCredentialAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserWithMfaSettingsAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.MfaSettings)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }
}
