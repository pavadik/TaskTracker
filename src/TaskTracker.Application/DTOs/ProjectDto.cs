namespace TaskTracker.Application.DTOs;

/// <summary>
/// Project data transfer object
/// </summary>
public record ProjectDto(
    Guid Id,
    string Name,
    string Slug,
    string Prefix,
    string? Description,
    string? IconUrl,
    Guid WorkspaceId,
    DateTime CreatedAt);

/// <summary>
/// Project summary DTO
/// </summary>
public record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Prefix,
    string? IconUrl,
    int TaskCount);
