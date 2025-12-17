using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a task is assigned
/// </summary>
public sealed class TaskAssignedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    public string FriendlyId { get; }
    public Guid ProjectId { get; }
    public Guid? AssigneeId { get; }
    public string? AssigneeName { get; }
    public DateTime OccurredOn { get; }

    public TaskAssignedEvent(Guid taskId, string friendlyId, Guid projectId, Guid? assigneeId, string? assigneeName)
    {
        TaskId = taskId;
        FriendlyId = friendlyId;
        ProjectId = projectId;
        AssigneeId = assigneeId;
        AssigneeName = assigneeName;
        OccurredOn = DateTime.UtcNow;
    }
}
