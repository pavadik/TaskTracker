using MediatR;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Features.Tasks.Commands;

/// <summary>
/// Command to update a task
/// </summary>
public record UpdateTaskCommand(
    Guid TaskId,
    string? Title = null,
    string? Description = null,
    TaskPriority? Priority = null,
    Guid? AssigneeId = null,
    DateTime? DueDate = null,
    int? StoryPoints = null,
    Guid? SprintId = null,
    string? CustomFields = null) : IRequest<TaskDto>;
