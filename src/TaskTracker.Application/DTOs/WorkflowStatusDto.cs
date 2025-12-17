using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs;

/// <summary>
/// Workflow status DTO
/// </summary>
public record WorkflowStatusDto(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    StatusCategory Category,
    int Order,
    bool IsDefault);

/// <summary>
/// Status with transitions
/// </summary>
public record WorkflowStatusWithTransitionsDto(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    StatusCategory Category,
    int Order,
    bool IsDefault,
    IReadOnlyList<StatusTransitionDto> Transitions);

/// <summary>
/// Status transition DTO
/// </summary>
public record StatusTransitionDto(
    Guid Id,
    Guid ToStatusId,
    string ToStatusName,
    string? Name,
    bool RequiresComment);
