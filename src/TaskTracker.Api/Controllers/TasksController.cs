using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Features.Tasks.Commands;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get tasks with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] GetTasksQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var taskId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = taskId }, new { id = taskId });
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            request.Priority,
            request.AssigneeId,
            request.DueDate,
            request.StoryPoints,
            request.SprintId,
            request.CustomFields);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Change task status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeTaskStatusCommand(id, request.NewStatusId, request.Comment);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Assign task to a user
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(id, AssigneeId: request.AssigneeId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Change task priority
    /// </summary>
    [HttpPatch("{id:guid}/priority")]
    public async Task<IActionResult> ChangePriority(Guid id, [FromBody] ChangePriorityRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(id, Priority: request.Priority);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record ChangeStatusRequest(Guid NewStatusId, string? Comment);
public record AssignTaskRequest(Guid? AssigneeId);
public record ChangePriorityRequest(TaskPriority Priority);
public record UpdateTaskRequest(
    string? Title = null,
    string? Description = null,
    TaskPriority? Priority = null,
    Guid? AssigneeId = null,
    DateTime? DueDate = null,
    int? StoryPoints = null,
    Guid? SprintId = null,
    string? CustomFields = null);
