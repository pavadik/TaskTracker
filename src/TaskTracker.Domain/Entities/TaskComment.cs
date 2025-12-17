using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Task comment entity
/// </summary>
public class TaskComment : AuditableEntity
{
    public string Content { get; private set; } = string.Empty;
    
    public Guid TaskId { get; private set; }
    public TaskItem Task { get; private set; } = null!;
    
    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;

    private TaskComment() { }

    public static Result<TaskComment> Create(TaskItem task, User author, string content, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<TaskComment>("Comment content cannot be empty");

        if (content.Length > 10000)
            return Result.Failure<TaskComment>("Comment content cannot exceed 10000 characters");

        var comment = new TaskComment
        {
            TaskId = task.Id,
            Task = task,
            AuthorId = author.Id,
            Author = author,
            Content = content.Trim()
        };

        comment.SetCreated(createdBy);
        return Result.Success(comment);
    }

    public Result Update(string content, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure("Comment content cannot be empty");

        if (content.Length > 10000)
            return Result.Failure("Comment content cannot exceed 10000 characters");

        Content = content.Trim();
        SetUpdated(updatedBy);
        return Result.Success();
    }
}
