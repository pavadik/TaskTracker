using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Task history entry for audit log
/// </summary>
public class TaskHistory : Entity
{
    public Guid TaskId { get; private set; }
    public TaskItem Task { get; private set; } = null!;
    
    public string FieldName { get; private set; } = string.Empty;
    public string OldValue { get; private set; } = string.Empty;
    public string NewValue { get; private set; } = string.Empty;
    
    public Guid ChangedById { get; private set; }
    public User ChangedBy { get; private set; } = null!;
    
    public DateTime ChangedAt { get; private set; }

    private TaskHistory() { }

    public static Result<TaskHistory> Create(
        TaskItem task, 
        string fieldName, 
        string oldValue, 
        string newValue,
        Guid changedBy)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure<TaskHistory>("Field name cannot be empty");

        var history = new TaskHistory
        {
            TaskId = task.Id,
            Task = task,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedById = changedBy,
            ChangedAt = DateTime.UtcNow
        };

        return Result.Success(history);
    }
}
