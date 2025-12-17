using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Features.Tasks.Commands;

/// <summary>
/// Command to create a new task
/// </summary>
public record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskType Type,
    TaskPriority Priority = TaskPriority.None,
    Guid? AssigneeId = null,
    Guid? ParentTaskId = null,
    Guid? SprintId = null,
    DateTime? DueDate = null,
    int? StoryPoints = null,
    string? CustomFields = null) : IRequest<TaskDto>;
