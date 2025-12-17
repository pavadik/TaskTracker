using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a task status changes
/// </summary>
public sealed class TaskStatusChangedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    public string FriendlyId { get; }
    public Guid ProjectId { get; }
    public Guid OldStatusId { get; }
    public string OldStatusName { get; }
    public Guid NewStatusId { get; }
    public string NewStatusName { get; }
    public DateTime OccurredOn { get; }

    public TaskStatusChangedEvent(
        Guid taskId, 
        string friendlyId, 
        Guid projectId, 
        Guid oldStatusId, 
        string oldStatusName,
        Guid newStatusId, 
        string newStatusName)
    {
        TaskId = taskId;
        FriendlyId = friendlyId;
        ProjectId = projectId;
        OldStatusId = oldStatusId;
        OldStatusName = oldStatusName;
        NewStatusId = newStatusId;
        NewStatusName = newStatusName;
        OccurredOn = DateTime.UtcNow;
    }
}
