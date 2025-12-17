using MediatR;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Features.Tasks.Commands;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;

namespace TaskTracker.Application.Features.Tasks.Handlers;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public CreateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId 
            ?? throw new ForbiddenAccessException("User must be authenticated");

        // Get project with default status
        var project = await _unitOfWork.Projects.GetWithStatusesAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        // Get reporter (current user)
        var reporter = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        // Get default status
        var defaultStatus = project.Statuses.FirstOrDefault(s => s.IsDefault)
            ?? project.Statuses.FirstOrDefault()
            ?? throw new BusinessRuleException("Project has no workflow statuses configured");

        // Get assignee if provided
        User? assignee = null;
        if (request.AssigneeId.HasValue)
        {
            assignee = await _unitOfWork.Users.GetByIdAsync(request.AssigneeId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.AssigneeId.Value);
        }

        // Get parent task if provided
        TaskItem? parentTask = null;
        if (request.ParentTaskId.HasValue)
        {
            parentTask = await _unitOfWork.Tasks.GetByIdAsync(request.ParentTaskId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(TaskItem), request.ParentTaskId.Value);

            if (parentTask.ProjectId != request.ProjectId)
                throw new BusinessRuleException("Parent task must belong to the same project");
        }

        // Create task
        var taskResult = TaskItem.Create(
            project,
            request.Title,
            defaultStatus,
            reporter,
            request.Type,
            userId,
            request.Description,
            request.Priority,
            assignee,
            parentTask);

        if (taskResult.IsFailure)
            throw new BusinessRuleException(taskResult.Error);

        var task = taskResult.Value;

        // Set optional fields
        if (request.DueDate.HasValue)
            task.SetDueDate(request.DueDate, userId);

        if (request.StoryPoints.HasValue)
            task.SetStoryPoints(request.StoryPoints, userId);

        if (!string.IsNullOrEmpty(request.CustomFields))
            task.SetCustomFields(request.CustomFields, userId);

        // Set sprint if provided
        if (request.SprintId.HasValue)
        {
            // TODO: Add sprint repository and set sprint
        }

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send real-time notification
        await _notificationService.SendToProjectAsync(
            project.Id,
            "TaskCreated",
            new { TaskId = task.Id, FriendlyId = task.FriendlyId.Value },
            cancellationToken);

        return MapToDto(task);
    }

    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto(
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
            task.Status.Name,
            task.Status.Color,
            task.AssigneeId,
            task.Assignee?.DisplayName,
            task.Assignee?.AvatarUrl,
            task.ReporterId,
            task.Reporter.DisplayName,
            task.ParentTaskId,
            task.SprintId,
            task.Sprint?.Name,
            task.CustomFields,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
