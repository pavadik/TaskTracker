namespace TaskTracker.Application.Common.Interfaces;

/// <summary>
/// Interface for accessing current user information
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
