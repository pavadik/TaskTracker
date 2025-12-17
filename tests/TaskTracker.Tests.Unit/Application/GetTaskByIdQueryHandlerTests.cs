using FluentAssertions;
using Moq;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Features.Tasks.Handlers;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Tests.Unit.Application;

public class GetTaskByIdQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetTaskByIdQueryHandler _handler;

    public GetTaskByIdQueryHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetTaskByIdQueryHandler(_taskRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTaskInCache_ShouldReturnCachedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var cachedDto = new TaskDto(
            taskId, "TEST-1", "Cached Task", null, TaskPriority.Medium, TaskType.Task,
            null, null, null, null, Guid.NewGuid(), Guid.NewGuid(), "To Do", "#999999",
            null, null, null, Guid.NewGuid(), "Reporter", null, null, null, null,
            DateTime.UtcNow, DateTime.UtcNow);

        _cacheServiceMock
            .Setup(x => x.GetAsync<TaskDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        var query = new GetTaskByIdQuery(taskId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(cachedDto);
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTaskNotInCacheButExists_ShouldReturnFromRepositoryAndCache()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        
        _cacheServiceMock
            .Setup(x => x.GetAsync<TaskDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskDto?)null);

        var task = CreateTestTask(taskId);
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(taskId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        _cacheServiceMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<TaskDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        _cacheServiceMock
            .Setup(x => x.GetAsync<TaskDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskDto?)null);

        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var query = new GetTaskByIdQuery(taskId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private TaskItem CreateTestTask(Guid taskId)
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = User.Create(Email.Create("test@test.com").Value, "Test User", Guid.NewGuid()).Value;
        
        var task = TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
        
        // We need to set the ID via reflection for testing purposes
        typeof(TaskItem).BaseType!.GetProperty("Id")!.SetValue(task, taskId);
        
        return task;
    }
}
