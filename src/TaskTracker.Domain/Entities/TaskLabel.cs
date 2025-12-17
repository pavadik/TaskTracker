using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Many-to-many relationship between tasks and labels
/// </summary>
public class TaskLabel : Entity
{
    public Guid TaskId { get; private set; }
    public TaskItem Task { get; private set; } = null!;
    
    public Guid LabelId { get; private set; }
    public Label Label { get; private set; } = null!;

    private TaskLabel() { }

    public static Result<TaskLabel> Create(TaskItem task, Label label)
    {
        if (task.ProjectId != label.ProjectId)
            return Result.Failure<TaskLabel>("Task and label must belong to the same project");

        var taskLabel = new TaskLabel
        {
            TaskId = task.Id,
            Task = task,
            LabelId = label.Id,
            Label = label
        };

        return Result.Success(taskLabel);
    }
}
