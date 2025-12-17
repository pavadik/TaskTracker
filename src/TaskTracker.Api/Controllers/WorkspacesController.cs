using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Repositories;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(
        IMediator mediator,
        IWorkspaceRepository workspaceRepository,
        ILogger<WorkspacesController> logger)
    {
        _mediator = mediator;
        _workspaceRepository = workspaceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all workspaces for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkspaces(CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceRepository.GetAllAsync(cancellationToken);
        var result = workspaces.Select(w => new WorkspaceDto(
            w.Id,
            w.Name,
            w.Slug.Value,
            w.Description,
            null,
            w.CreatedAt
        ));
        return Ok(result);
    }

    /// <summary>
    /// Get a specific workspace by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWorkspace(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id, cancellationToken);
        if (workspace == null)
            return NotFound();

        var result = new WorkspaceDto(
            workspace.Id,
            workspace.Name,
            workspace.Slug.Value,
            workspace.Description,
            null,
            workspace.CreatedAt
        );
        return Ok(result);
    }

    /// <summary>
    /// Create a new workspace
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement via MediatR command
        return StatusCode(501, "Not implemented");
    }

    /// <summary>
    /// Update a workspace
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement via MediatR command
        return StatusCode(501, "Not implemented");
    }

    /// <summary>
    /// Delete a workspace
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWorkspace(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement via MediatR command
        return StatusCode(501, "Not implemented");
    }

    /// <summary>
    /// Get workspace members
    /// </summary>
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetWithMembersAsync(id, cancellationToken);
        if (workspace == null)
            return NotFound();

        var result = workspace.Members.Select(m => new WorkspaceMemberDto
        {
            UserId = m.UserId,
            Role = m.Role,
            JoinedAt = m.CreatedAt
        });
        return Ok(result);
    }
}

public record CreateWorkspaceRequest(string Name, string? Description);
public record UpdateWorkspaceRequest(string Name, string? Description);
public record WorkspaceMemberDto
{
    public Guid UserId { get; init; }
    public WorkspaceRole Role { get; init; }
    public DateTime JoinedAt { get; init; }
}
