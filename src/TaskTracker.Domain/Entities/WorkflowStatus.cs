using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Workflow status entity - defines a state in the task workflow
/// </summary>
public class WorkflowStatus : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Color { get; private set; } = "#808080";
    public StatusCategory Category { get; private set; }
    public int Order { get; private set; }
    public bool IsDefault { get; private set; }
    
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    private readonly List<StatusTransition> _outgoingTransitions = new();
    public IReadOnlyCollection<StatusTransition> OutgoingTransitions => _outgoingTransitions.AsReadOnly();

    private readonly List<StatusTransition> _incomingTransitions = new();
    public IReadOnlyCollection<StatusTransition> IncomingTransitions => _incomingTransitions.AsReadOnly();

    private WorkflowStatus() { }

    public static Result<WorkflowStatus> Create(
        Project project,
        string name,
        StatusCategory category,
        int order,
        Guid createdBy,
        string? description = null,
        string color = "#808080",
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<WorkflowStatus>("Status name cannot be empty");

        if (name.Length > 50)
            return Result.Failure<WorkflowStatus>("Status name cannot exceed 50 characters");

        var status = new WorkflowStatus
        {
            ProjectId = project.Id,
            Project = project,
            Name = name.Trim(),
            Description = description?.Trim(),
            Category = category,
            Color = color,
            Order = order,
            IsDefault = isDefault
        };

        status.SetCreated(createdBy);
        return Result.Success(status);
    }

    public Result Update(string name, string? description, string color, StatusCategory category, int order, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Status name cannot be empty");

        if (name.Length > 50)
            return Result.Failure("Status name cannot exceed 50 characters");

        Name = name.Trim();
        Description = description?.Trim();
        Color = color;
        Category = category;
        Order = order;
        SetUpdated(updatedBy);

        return Result.Success();
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
    }

    public void AddOutgoingTransition(StatusTransition transition)
    {
        _outgoingTransitions.Add(transition);
    }
}
