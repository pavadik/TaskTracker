using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(TaskTrackerDbContext context) : base(context)
    {
    }

    public async Task<TaskItem?> GetByFriendlyIdAsync(FriendlyId friendlyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .FirstOrDefaultAsync(t => 
                t.FriendlyId.ProjectPrefix == friendlyId.ProjectPrefix && 
                t.FriendlyId.SequenceNumber == friendlyId.SequenceNumber, 
                cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.SprintId == sprintId)
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.AssigneeId == assigneeId)
            .Include(t => t.Status)
            .Include(t => t.Project)
            .OrderByDescending(t => t.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Status)
                .ThenInclude(s => s.OutgoingTransitions)
                    .ThenInclude(tr => tr.ToStatus)
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .Include(t => t.Sprint)
            .Include(t => t.Comments.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.Author)
            .Include(t => t.Attachments.Where(a => !a.IsDeleted))
            .Include(t => t.Labels)
                .ThenInclude(l => l.Label)
            .Include(t => t.Subtasks.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetSubtasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.ParentTaskId == parentTaskId)
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid projectId,
        int page,
        int pageSize,
        Guid? statusId = null,
        Guid? assigneeId = null,
        Guid? sprintId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(t => t.ProjectId == projectId);

        if (statusId.HasValue)
            query = query.Where(t => t.StatusId == statusId.Value);

        if (assigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == assigneeId.Value);

        if (sprintId.HasValue)
            query = query.Where(t => t.SprintId == sprintId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(t => 
                t.Title.ToLower().Contains(term) || 
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
