using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a task is created
/// </summary>
public sealed class TaskCreatedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    public string FriendlyId { get; }
    public Guid ProjectId { get; }
    public string Title { get; }
    public TaskType Type { get; }
    public Guid StatusId { get; }
    public DateTime OccurredOn { get; }

    public TaskCreatedEvent(Guid taskId, string friendlyId, Guid projectId, string title, TaskType type, Guid statusId)
    {
        TaskId = taskId;
        FriendlyId = friendlyId;
        ProjectId = projectId;
        Title = title;
        Type = type;
        StatusId = statusId;
        OccurredOn = DateTime.UtcNow;
    }
}
