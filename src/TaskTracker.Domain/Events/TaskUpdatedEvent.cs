using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a task is updated
/// </summary>
public sealed class TaskUpdatedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    public string FriendlyId { get; }
    public Guid ProjectId { get; }
    public string FieldName { get; }
    public string OldValue { get; }
    public string NewValue { get; }
    public DateTime OccurredOn { get; }

    public TaskUpdatedEvent(Guid taskId, string friendlyId, Guid projectId, string fieldName, string oldValue, string newValue)
    {
        TaskId = taskId;
        FriendlyId = friendlyId;
        ProjectId = projectId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        OccurredOn = DateTime.UtcNow;
    }
}
