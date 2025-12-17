using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Represents a user's membership in a workspace
/// </summary>
public class WorkspaceMember : AuditableEntity
{
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public WorkspaceRole Role { get; private set; }

    private WorkspaceMember() { }

    public static Result<WorkspaceMember> Create(Workspace workspace, User user, WorkspaceRole role, Guid createdBy)
    {
        var member = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = user.Id,
            User = user,
            Role = role
        };

        member.SetCreated(createdBy);
        return Result.Success(member);
    }

    public Result ChangeRole(WorkspaceRole newRole, Guid changedBy)
    {
        if (Role == WorkspaceRole.Owner && newRole != WorkspaceRole.Owner)
            return Result.Failure("Cannot demote the workspace owner");

        Role = newRole;
        SetUpdated(changedBy);
        return Result.Success();
    }
}
