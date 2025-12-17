using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TaskTrackerDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(u => u.Email == email, cancellationToken);
    }
}
