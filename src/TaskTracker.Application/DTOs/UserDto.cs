namespace TaskTracker.Application.DTOs;

/// <summary>
/// User data transfer object
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    bool IsActive,
    DateTime CreatedAt);
