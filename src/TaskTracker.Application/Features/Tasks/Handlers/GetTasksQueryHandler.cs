using MediatR;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Models;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;

namespace TaskTracker.Application.Features.Tasks.Handlers;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PaginatedList<TaskSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<TaskSummaryDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        // Verify project exists
        var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var (tasks, totalCount) = await _unitOfWork.Tasks.GetPagedAsync(
            request.ProjectId,
            request.Page,
            request.PageSize,
            request.StatusId,
            request.AssigneeId,
            request.SprintId,
            request.SearchTerm,
            cancellationToken);

        var items = tasks.Select(MapToSummaryDto).ToList();

        return new PaginatedList<TaskSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }

    private static TaskSummaryDto MapToSummaryDto(TaskItem task)
    {
        return new TaskSummaryDto(
            task.Id,
            task.FriendlyId.Value,
            task.Title,
            task.Priority,
            task.Type,
            task.StatusId,
            task.Status.Name,
            task.Status.Color,
            task.AssigneeId,
            task.Assignee?.DisplayName,
            task.Assignee?.AvatarUrl,
            task.DueDate,
            task.StoryPoints);
    }
}
