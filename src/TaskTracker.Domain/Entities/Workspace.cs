using TaskTracker.Domain.Common;
using TaskTracker.Domain.Events;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Workspace entity (Tenant) - root container for an organization
/// </summary>
public class Workspace : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }

    private readonly List<Project> _projects = new();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    private readonly List<WorkspaceMember> _members = new();
    public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();

    private Workspace() { }

    public static Result<Workspace> Create(string name, Slug slug, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Workspace>("Workspace name cannot be empty");

        if (name.Length > 100)
            return Result.Failure<Workspace>("Workspace name cannot exceed 100 characters");

        var workspace = new Workspace
        {
            Name = name.Trim(),
            Slug = slug
        };

        workspace.SetCreated(ownerId);
        workspace.AddDomainEvent(new WorkspaceCreatedEvent(workspace.Id, name, slug.Value, ownerId));

        return Result.Success(workspace);
    }

    public Result Update(string name, string? description, string? logoUrl, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Workspace name cannot be empty");

        if (name.Length > 100)
            return Result.Failure("Workspace name cannot exceed 100 characters");

        Name = name.Trim();
        Description = description?.Trim();
        LogoUrl = logoUrl?.Trim();
        SetUpdated(updatedBy);

        return Result.Success();
    }

    public Result AddMember(User user, Enums.WorkspaceRole role, Guid addedBy)
    {
        if (_members.Any(m => m.UserId == user.Id && !m.IsDeleted))
            return Result.Failure("User is already a member of this workspace");

        var memberResult = WorkspaceMember.Create(this, user, role, addedBy);
        if (memberResult.IsFailure)
            return Result.Failure(memberResult.Error);

        _members.Add(memberResult.Value);
        return Result.Success();
    }

    public Result RemoveMember(Guid userId, Guid removedBy)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted);
        if (member is null)
            return Result.Failure("User is not a member of this workspace");

        if (member.Role == Enums.WorkspaceRole.Owner)
            return Result.Failure("Cannot remove the workspace owner");

        member.MarkAsDeleted(removedBy);
        return Result.Success();
    }

    public void AddProject(Project project)
    {
        _projects.Add(project);
    }
}
