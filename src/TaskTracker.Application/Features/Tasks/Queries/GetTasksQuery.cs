using MediatR;
using TaskTracker.Application.Common.Models;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Features.Tasks.Queries;

/// <summary>
/// Query to get paginated tasks for a project
/// </summary>
public record GetTasksQuery(
    Guid ProjectId,
    int Page = 1,
    int PageSize = 20,
    Guid? StatusId = null,
    Guid? AssigneeId = null,
    Guid? SprintId = null,
    string? SearchTerm = null) : IRequest<PaginatedList<TaskSummaryDto>>;
