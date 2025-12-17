using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Repositories;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IMediator mediator,
        IProjectRepository projectRepository,
        ILogger<ProjectsController> logger)
    {
        _mediator = mediator;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects in a workspace
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjects(Guid workspaceId, CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var result = projects.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.Slug.Value,
            p.Prefix,
            p.Description,
            p.IconUrl,
            p.WorkspaceId,
            p.CreatedAt
        ));
        return Ok(result);
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid workspaceId, Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null || project.WorkspaceId != workspaceId)
            return NotFound();

        var result = new ProjectDto(
            project.Id,
            project.Name,
            project.Slug.Value,
            project.Prefix,
            project.Description,
            project.IconUrl,
            project.WorkspaceId,
            project.CreatedAt
        );
        return Ok(result);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProject(Guid workspaceId, [FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement via MediatR command
        return StatusCode(501, "Not implemented");
    }

    /// <summary>
    /// Update a project
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid workspaceId, Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement via MediatR command
        return StatusCode(501, "Not implemented");
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid workspaceId, Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement via MediatR command
        return StatusCode(501, "Not implemented");
    }

    /// <summary>
    /// Get project workflow statuses
    /// </summary>
    [HttpGet("{id:guid}/statuses")]
    public async Task<IActionResult> GetStatuses(Guid workspaceId, Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetWithStatusesAsync(id, cancellationToken);
        if (project == null || project.WorkspaceId != workspaceId)
            return NotFound();

        var result = project.Statuses
            .OrderBy(s => s.Order)
            .Select(s => new WorkflowStatusDto(
                s.Id,
                s.Name,
                s.Description,
                s.Color,
                s.Category,
                s.Order,
                s.IsDefault
            ));
        return Ok(result);
    }
}

public record CreateProjectRequest(string Name, string Prefix, string? Description);
public record UpdateProjectRequest(string Name, string? Description);
