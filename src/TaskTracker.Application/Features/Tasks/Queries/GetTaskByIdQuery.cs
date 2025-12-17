using MediatR;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Features.Tasks.Queries;

/// <summary>
/// Query to get a task by ID
/// </summary>
public record GetTaskByIdQuery(Guid TaskId) : IRequest<TaskDto?>;
