using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<TaskHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string eventType, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"user:{userId}")
                .SendAsync(eventType, payload, cancellationToken);
            
            _logger.LogInformation("Notification sent to user {UserId}: {EventType}", userId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
        }
    }

    public async Task SendToWorkspaceAsync(Guid workspaceId, string eventType, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"workspace:{workspaceId}")
                .SendAsync(eventType, payload, cancellationToken);
            
            _logger.LogInformation("Notification sent to workspace {WorkspaceId}: {EventType}", workspaceId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to workspace {WorkspaceId}", workspaceId);
        }
    }

    public async Task SendToProjectAsync(Guid projectId, string eventType, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"project:{projectId}")
                .SendAsync(eventType, payload, cancellationToken);
            
            _logger.LogInformation("Notification sent to project {ProjectId}: {EventType}", projectId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to project {ProjectId}", projectId);
        }
    }
}
