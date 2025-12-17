using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(TaskTrackerDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetBySlugAsync(Guid workspaceId, Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.WorkspaceId == workspaceId && p.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.WorkspaceId == workspaceId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid workspaceId, Slug slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(p => p.WorkspaceId == workspaceId && p.Slug == slug, cancellationToken);
    }

    public async Task<bool> PrefixExistsAsync(Guid workspaceId, string prefix, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(p => p.WorkspaceId == workspaceId && p.Prefix == prefix.ToUpperInvariant(), cancellationToken);
    }

    public async Task<Project?> GetWithStatusesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Statuses.Where(s => !s.IsDeleted).OrderBy(s => s.Order))
                .ThenInclude(s => s.OutgoingTransitions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project?> GetWithCustomFieldsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.CustomFields.Where(f => !f.IsDeleted).OrderBy(f => f.Order))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
