using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Repositories;

public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(TaskTrackerDbContext context) : base(context)
    {
    }

    public async Task<Workspace?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(w => w.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(w => w.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(w => w.Slug == slug, cancellationToken);
    }

    public async Task<Workspace?> GetWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }
}
