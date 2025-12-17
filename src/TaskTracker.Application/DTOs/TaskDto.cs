using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs;

/// <summary>
/// Task data transfer object
/// </summary>
public record TaskDto(
    Guid Id,
    string FriendlyId,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskType Type,
    int? StoryPoints,
    DateTime? DueDate,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    Guid ProjectId,
    Guid StatusId,
    string StatusName,
    string StatusColor,
    Guid? AssigneeId,
    string? AssigneeName,
    string? AssigneeAvatarUrl,
    Guid ReporterId,
    string ReporterName,
    Guid? ParentTaskId,
    Guid? SprintId,
    string? SprintName,
    string? CustomFields,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Task summary for lists
/// </summary>
public record TaskSummaryDto(
    Guid Id,
    string FriendlyId,
    string Title,
    TaskPriority Priority,
    TaskType Type,
    Guid StatusId,
    string StatusName,
    string StatusColor,
    Guid? AssigneeId,
    string? AssigneeName,
    string? AssigneeAvatarUrl,
    DateTime? DueDate,
    int? StoryPoints);

/// <summary>
/// Task for board view (Kanban)
/// </summary>
public record TaskBoardItemDto(
    Guid Id,
    string FriendlyId,
    string Title,
    TaskPriority Priority,
    TaskType Type,
    Guid? AssigneeId,
    string? AssigneeName,
    string? AssigneeAvatarUrl,
    IReadOnlyList<LabelDto> Labels,
    int SubtaskCount,
    int SubtaskDoneCount,
    int CommentCount);
