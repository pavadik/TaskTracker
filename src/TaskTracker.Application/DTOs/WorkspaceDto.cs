namespace TaskTracker.Application.DTOs;

/// <summary>
/// Workspace data transfer object
/// </summary>
public record WorkspaceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    DateTime CreatedAt);

/// <summary>
/// Workspace summary with member count
/// </summary>
public record WorkspaceSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    int MemberCount,
    int ProjectCount);
