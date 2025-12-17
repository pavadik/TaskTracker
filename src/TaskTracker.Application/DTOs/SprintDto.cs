namespace TaskTracker.Application.DTOs;

/// <summary>
/// Sprint DTO
/// </summary>
public record SprintDto(
    Guid Id,
    string Name,
    string? Goal,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCompleted,
    Guid ProjectId,
    DateTime CreatedAt);

/// <summary>
/// Sprint with metrics
/// </summary>
public record SprintWithMetricsDto(
    Guid Id,
    string Name,
    string? Goal,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCompleted,
    int TotalTasks,
    int CompletedTasks,
    int TotalStoryPoints,
    int CompletedStoryPoints);
