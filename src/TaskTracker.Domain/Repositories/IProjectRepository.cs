using TaskTracker.Domain.Entities;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Repositories;

/// <summary>
/// Repository for Project entity
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetBySlugAsync(Guid workspaceId, Slug slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid workspaceId, Slug slug, CancellationToken cancellationToken = default);
    Task<bool> PrefixExistsAsync(Guid workspaceId, string prefix, CancellationToken cancellationToken = default);
    Task<Project?> GetWithStatusesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetWithCustomFieldsAsync(Guid id, CancellationToken cancellationToken = default);
}
