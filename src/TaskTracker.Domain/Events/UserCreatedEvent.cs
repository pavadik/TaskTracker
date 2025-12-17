using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Events;

/// <summary>
/// Event raised when a new user is created
/// </summary>
public sealed class UserCreatedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string DisplayName { get; }
    public DateTime OccurredOn { get; }

    public UserCreatedEvent(Guid userId, string email, string displayName)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
        OccurredOn = DateTime.UtcNow;
    }
}
