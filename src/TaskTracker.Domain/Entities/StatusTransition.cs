using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Status transition - defines allowed transitions between workflow statuses
/// </summary>
public class StatusTransition : AuditableEntity
{
    public Guid FromStatusId { get; private set; }
    public WorkflowStatus FromStatus { get; private set; } = null!;
    
    public Guid ToStatusId { get; private set; }
    public WorkflowStatus ToStatus { get; private set; } = null!;
    
    public string? Name { get; private set; }
    
    /// <summary>
    /// Optional: User ID to auto-assign when this transition occurs
    /// </summary>
    public Guid? AutoAssignUserId { get; private set; }
    
    /// <summary>
    /// Whether this transition requires a comment
    /// </summary>
    public bool RequiresComment { get; private set; }

    private StatusTransition() { }

    public static Result<StatusTransition> Create(
        WorkflowStatus fromStatus,
        WorkflowStatus toStatus,
        Guid createdBy,
        string? name = null,
        Guid? autoAssignUserId = null,
        bool requiresComment = false)
    {
        if (fromStatus.ProjectId != toStatus.ProjectId)
            return Result.Failure<StatusTransition>("Statuses must belong to the same project");

        if (fromStatus.Id == toStatus.Id)
            return Result.Failure<StatusTransition>("Cannot create transition to the same status");

        var transition = new StatusTransition
        {
            FromStatusId = fromStatus.Id,
            FromStatus = fromStatus,
            ToStatusId = toStatus.Id,
            ToStatus = toStatus,
            Name = name?.Trim(),
            AutoAssignUserId = autoAssignUserId,
            RequiresComment = requiresComment
        };

        transition.SetCreated(createdBy);
        return Result.Success(transition);
    }

    public void Update(string? name, Guid? autoAssignUserId, bool requiresComment, Guid updatedBy)
    {
        Name = name?.Trim();
        AutoAssignUserId = autoAssignUserId;
        RequiresComment = requiresComment;
        SetUpdated(updatedBy);
    }
}
