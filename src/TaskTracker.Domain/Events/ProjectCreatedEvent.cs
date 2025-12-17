using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a project is created
/// </summary>
public sealed class ProjectCreatedEvent : IDomainEvent
{
    public Guid ProjectId { get; }
    public Guid WorkspaceId { get; }
    public string Name { get; }
    public string Slug { get; }
    public string Prefix { get; }
    public DateTime OccurredOn { get; }

    public ProjectCreatedEvent(Guid projectId, Guid workspaceId, string name, string slug, string prefix)
    {
        ProjectId = projectId;
        WorkspaceId = workspaceId;
        Name = name;
        Slug = slug;
        Prefix = prefix;
        OccurredOn = DateTime.UtcNow;
    }
}
