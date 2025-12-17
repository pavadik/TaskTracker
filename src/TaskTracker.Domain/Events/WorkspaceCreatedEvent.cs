using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a workspace is created
/// </summary>
public sealed class WorkspaceCreatedEvent : IDomainEvent
{
    public Guid WorkspaceId { get; }
    public string Name { get; }
    public string Slug { get; }
    public Guid OwnerId { get; }
    public DateTime OccurredOn { get; }

    public WorkspaceCreatedEvent(Guid workspaceId, string name, string slug, Guid ownerId)
    {
        WorkspaceId = workspaceId;
        Name = name;
        Slug = slug;
        OwnerId = ownerId;
        OccurredOn = DateTime.UtcNow;
    }
}
