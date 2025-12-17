namespace TaskTracker.Application.Common.Interfaces;

/// <summary>
/// Interface for real-time notification service
/// </summary>
public interface INotificationService
{
    Task SendToProjectAsync(Guid projectId, string eventType, object payload, CancellationToken cancellationToken = default);
    Task SendToUserAsync(Guid userId, string eventType, object payload, CancellationToken cancellationToken = default);
    Task SendToWorkspaceAsync(Guid workspaceId, string eventType, object payload, CancellationToken cancellationToken = default);
}
