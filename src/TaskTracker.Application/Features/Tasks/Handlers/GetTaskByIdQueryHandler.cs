using MediatR;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Repositories;

namespace TaskTracker.Application.Features.Tasks.Handlers;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICacheService _cacheService;

    public GetTaskByIdQueryHandler(
        ITaskRepository taskRepository,
        ICacheService cacheService)
    {
        _taskRepository = taskRepository;
        _cacheService = cacheService;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"task:{request.TaskId}";
        
        var cached = await _cacheService.GetAsync<TaskDto>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        
        if (task == null)
            throw new NotFoundException("Task", request.TaskId);

        var dto = new TaskDto(
            task.Id,
            task.FriendlyId.Value,
            task.Title,
            task.Description,
            task.Priority,
            task.Type,
            task.StoryPoints,
            task.DueDate,
            task.StartedAt,
            task.CompletedAt,
            task.ProjectId,
            task.StatusId,
            task.Status?.Name ?? "Unknown",
            task.Status?.Color ?? "#999999",
            task.AssigneeId,
            task.Assignee?.DisplayName,
            task.Assignee?.AvatarUrl,
            task.ReporterId,
            task.Reporter?.DisplayName ?? "Unknown",
            task.ParentTaskId,
            task.SprintId,
            task.Sprint?.Name,
            task.CustomFields,
            task.CreatedAt,
            task.UpdatedAt);

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);

        return dto;
    }
}
