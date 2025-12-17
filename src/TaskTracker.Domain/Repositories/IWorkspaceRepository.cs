using TaskTracker.Domain.Entities;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Repositories;

/// <summary>
/// Repository for Workspace entity
/// </summary>
public interface IWorkspaceRepository : IRepository<Workspace>
{
    Task<Workspace?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Slug slug, CancellationToken cancellationToken = default);
    Task<Workspace?> GetWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
}
