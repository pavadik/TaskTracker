using TaskTracker.Domain.Entities;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Repositories;

/// <summary>
/// Repository for TaskItem entity
/// </summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    Task<TaskItem?> GetByFriendlyIdAsync(FriendlyId friendlyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetSubtasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get tasks with pagination and filtering
    /// </summary>
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid projectId,
        int page,
        int pageSize,
        Guid? statusId = null,
        Guid? assigneeId = null,
        Guid? sprintId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
