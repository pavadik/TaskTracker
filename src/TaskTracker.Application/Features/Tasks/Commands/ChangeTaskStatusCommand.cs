using MediatR;

namespace TaskTracker.Application.Features.Tasks.Commands;

/// <summary>
/// Command to change task status
/// </summary>
public record ChangeTaskStatusCommand(
    Guid TaskId,
    Guid NewStatusId,
    string? Comment = null) : IRequest<Unit>;
