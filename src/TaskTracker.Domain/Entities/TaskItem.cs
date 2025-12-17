using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Events;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Task entity - the main work unit in the system
/// </summary>
public class TaskItem : AuditableEntity
{
    public FriendlyId FriendlyId { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskType Type { get; private set; }
    public int? StoryPoints { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    
    public Guid StatusId { get; private set; }
    public WorkflowStatus Status { get; private set; } = null!;
    
    public Guid? AssigneeId { get; private set; }
    public User? Assignee { get; private set; }
    
    public Guid ReporterId { get; private set; }
    public User Reporter { get; private set; } = null!;
    
    public Guid? ParentTaskId { get; private set; }
    public TaskItem? ParentTask { get; private set; }
    
    public Guid? SprintId { get; private set; }
    public Sprint? Sprint { get; private set; }

    /// <summary>
    /// Custom fields stored as JSONB
    /// </summary>
    public string? CustomFields { get; private set; }

    private readonly List<TaskItem> _subtasks = new();
    public IReadOnlyCollection<TaskItem> Subtasks => _subtasks.AsReadOnly();

    private readonly List<TaskComment> _comments = new();
    public IReadOnlyCollection<TaskComment> Comments => _comments.AsReadOnly();

    private readonly List<TaskAttachment> _attachments = new();
    public IReadOnlyCollection<TaskAttachment> Attachments => _attachments.AsReadOnly();

    private readonly List<TaskHistory> _history = new();
    public IReadOnlyCollection<TaskHistory> History => _history.AsReadOnly();

    private readonly List<TaskLabel> _labels = new();
    public IReadOnlyCollection<TaskLabel> Labels => _labels.AsReadOnly();

    private TaskItem() { }

    public static Result<TaskItem> Create(
        Project project,
        string title,
        WorkflowStatus status,
        User reporter,
        TaskType type,
        Guid createdBy,
        string? description = null,
        TaskPriority priority = TaskPriority.None,
        User? assignee = null,
        TaskItem? parentTask = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<TaskItem>("Task title cannot be empty");

        if (title.Length > 500)
            return Result.Failure<TaskItem>("Task title cannot exceed 500 characters");

        var taskNumber = project.GetAndIncrementTaskNumber();
        var friendlyIdResult = FriendlyId.Create(project.Prefix, taskNumber);
        if (friendlyIdResult.IsFailure)
            return Result.Failure<TaskItem>(friendlyIdResult.Error);

        var task = new TaskItem
        {
            FriendlyId = friendlyIdResult.Value,
            ProjectId = project.Id,
            Project = project,
            Title = title.Trim(),
            Description = description?.Trim(),
            StatusId = status.Id,
            Status = status,
            ReporterId = reporter.Id,
            Reporter = reporter,
            Type = type,
            Priority = priority,
            AssigneeId = assignee?.Id,
            Assignee = assignee,
            ParentTaskId = parentTask?.Id,
            ParentTask = parentTask
        };

        task.SetCreated(createdBy);
        task.AddDomainEvent(new TaskCreatedEvent(
            task.Id, 
            task.FriendlyId.Value, 
            project.Id, 
            title, 
            type,
            status.Id));

        return Result.Success(task);
    }

    public Result UpdateTitle(string title, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Task title cannot be empty");

        if (title.Length > 500)
            return Result.Failure("Task title cannot exceed 500 characters");

        var oldValue = Title;
        Title = title.Trim();
        SetUpdated(updatedBy);

        RecordChange(nameof(Title), oldValue, Title, updatedBy);
        AddDomainEvent(new TaskUpdatedEvent(Id, FriendlyId.Value, ProjectId, nameof(Title), oldValue, Title));

        return Result.Success();
    }

    public Result UpdateDescription(string? description, Guid updatedBy)
    {
        var oldValue = Description;
        Description = description?.Trim();
        SetUpdated(updatedBy);

        RecordChange(nameof(Description), oldValue ?? "", Description ?? "", updatedBy);
        AddDomainEvent(new TaskUpdatedEvent(Id, FriendlyId.Value, ProjectId, nameof(Description), oldValue ?? "", Description ?? ""));

        return Result.Success();
    }

    public Result ChangeStatus(WorkflowStatus newStatus, Guid changedBy, string? comment = null)
    {
        if (newStatus.ProjectId != ProjectId)
            return Result.Failure("Status does not belong to the same project");

        // Check if transition is allowed
        var transition = Status.OutgoingTransitions.FirstOrDefault(t => t.ToStatusId == newStatus.Id);
        if (transition is null)
            return Result.Failure($"Transition from '{Status.Name}' to '{newStatus.Name}' is not allowed");

        if (transition.RequiresComment && string.IsNullOrWhiteSpace(comment))
            return Result.Failure("This transition requires a comment");

        var oldStatus = Status;
        var oldStatusId = StatusId;

        StatusId = newStatus.Id;
        Status = newStatus;

        // Track started/completed timestamps
        if (newStatus.Category == StatusCategory.InProgress && StartedAt is null)
            StartedAt = DateTime.UtcNow;

        if (newStatus.Category == StatusCategory.Done && CompletedAt is null)
            CompletedAt = DateTime.UtcNow;

        // Auto-assign if configured
        if (transition.AutoAssignUserId.HasValue)
            AssigneeId = transition.AutoAssignUserId.Value;

        SetUpdated(changedBy);

        RecordChange(nameof(StatusId), oldStatusId.ToString(), StatusId.ToString(), changedBy);
        AddDomainEvent(new TaskStatusChangedEvent(
            Id, 
            FriendlyId.Value, 
            ProjectId, 
            oldStatusId, 
            oldStatus.Name,
            StatusId, 
            newStatus.Name));

        return Result.Success();
    }

    public Result ChangePriority(TaskPriority priority, Guid changedBy)
    {
        var oldValue = Priority;
        Priority = priority;
        SetUpdated(changedBy);

        RecordChange(nameof(Priority), oldValue.ToString(), Priority.ToString(), changedBy);
        AddDomainEvent(new TaskUpdatedEvent(Id, FriendlyId.Value, ProjectId, nameof(Priority), oldValue.ToString(), Priority.ToString()));

        return Result.Success();
    }

    public Result Assign(User? assignee, Guid assignedBy)
    {
        var oldValue = AssigneeId?.ToString() ?? "";
        AssigneeId = assignee?.Id;
        Assignee = assignee;
        SetUpdated(assignedBy);

        RecordChange(nameof(AssigneeId), oldValue, AssigneeId?.ToString() ?? "", assignedBy);
        AddDomainEvent(new TaskAssignedEvent(Id, FriendlyId.Value, ProjectId, assignee?.Id, assignee?.DisplayName));

        return Result.Success();
    }

    public Result SetDueDate(DateTime? dueDate, Guid updatedBy)
    {
        var oldValue = DueDate?.ToString("o") ?? "";
        DueDate = dueDate;
        SetUpdated(updatedBy);

        RecordChange(nameof(DueDate), oldValue, DueDate?.ToString("o") ?? "", updatedBy);

        return Result.Success();
    }

    public Result SetStoryPoints(int? storyPoints, Guid updatedBy)
    {
        if (storyPoints.HasValue && storyPoints.Value < 0)
            return Result.Failure("Story points cannot be negative");

        var oldValue = StoryPoints?.ToString() ?? "";
        StoryPoints = storyPoints;
        SetUpdated(updatedBy);

        RecordChange(nameof(StoryPoints), oldValue, StoryPoints?.ToString() ?? "", updatedBy);

        return Result.Success();
    }

    public Result SetSprint(Sprint? sprint, Guid updatedBy)
    {
        if (sprint is not null && sprint.ProjectId != ProjectId)
            return Result.Failure("Sprint does not belong to the same project");

        var oldValue = SprintId?.ToString() ?? "";
        SprintId = sprint?.Id;
        Sprint = sprint;
        SetUpdated(updatedBy);

        RecordChange(nameof(SprintId), oldValue, SprintId?.ToString() ?? "", updatedBy);

        return Result.Success();
    }

    public void SetCustomFields(string? customFieldsJson, Guid updatedBy)
    {
        var oldValue = CustomFields ?? "";
        CustomFields = customFieldsJson;
        SetUpdated(updatedBy);

        RecordChange(nameof(CustomFields), oldValue, CustomFields ?? "", updatedBy);
    }

    public void AddSubtask(TaskItem subtask)
    {
        _subtasks.Add(subtask);
    }

    public void AddComment(TaskComment comment)
    {
        _comments.Add(comment);
    }

    public void AddAttachment(TaskAttachment attachment)
    {
        _attachments.Add(attachment);
    }

    public void AddLabel(TaskLabel label)
    {
        _labels.Add(label);
    }

    public void RemoveLabel(TaskLabel label)
    {
        _labels.Remove(label);
    }

    private void RecordChange(string fieldName, string oldValue, string newValue, Guid changedBy)
    {
        var historyEntry = TaskHistory.Create(this, fieldName, oldValue, newValue, changedBy);
        if (historyEntry.IsSuccess)
            _history.Add(historyEntry.Value);
    }
}
