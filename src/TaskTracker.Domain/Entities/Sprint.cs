using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Sprint entity for Scrum methodology
/// </summary>
public class Sprint : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Goal { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsCompleted { get; private set; }
    
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    private readonly List<TaskItem> _tasks = new();
    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    private Sprint() { }

    public static Result<Sprint> Create(
        Project project,
        string name,
        DateTime startDate,
        DateTime endDate,
        Guid createdBy,
        string? goal = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Sprint>("Sprint name cannot be empty");

        if (name.Length > 100)
            return Result.Failure<Sprint>("Sprint name cannot exceed 100 characters");

        if (endDate <= startDate)
            return Result.Failure<Sprint>("End date must be after start date");

        var sprint = new Sprint
        {
            ProjectId = project.Id,
            Project = project,
            Name = name.Trim(),
            Goal = goal?.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsActive = false,
            IsCompleted = false
        };

        sprint.SetCreated(createdBy);
        return Result.Success(sprint);
    }

    public Result Update(string name, string? goal, DateTime startDate, DateTime endDate, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Sprint name cannot be empty");

        if (name.Length > 100)
            return Result.Failure("Sprint name cannot exceed 100 characters");

        if (endDate <= startDate)
            return Result.Failure("End date must be after start date");

        Name = name.Trim();
        Goal = goal?.Trim();
        StartDate = startDate;
        EndDate = endDate;
        SetUpdated(updatedBy);

        return Result.Success();
    }

    public Result Start(Guid startedBy)
    {
        if (IsActive)
            return Result.Failure("Sprint is already active");

        if (IsCompleted)
            return Result.Failure("Cannot start a completed sprint");

        IsActive = true;
        SetUpdated(startedBy);
        return Result.Success();
    }

    public Result Complete(Guid completedBy)
    {
        if (!IsActive)
            return Result.Failure("Cannot complete an inactive sprint");

        if (IsCompleted)
            return Result.Failure("Sprint is already completed");

        IsActive = false;
        IsCompleted = true;
        SetUpdated(completedBy);
        return Result.Success();
    }

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
    }

    public void RemoveTask(TaskItem task)
    {
        _tasks.Remove(task);
    }
}
