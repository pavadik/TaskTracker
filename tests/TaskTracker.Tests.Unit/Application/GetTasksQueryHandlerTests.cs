using FluentAssertions;
using Moq;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Common.Models;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Features.Tasks.Handlers;
using TaskTracker.Application.Features.Tasks.Queries;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Tests.Unit.Application;

public class GetTasksQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly GetTasksQueryHandler _handler;

    public GetTasksQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();

        _unitOfWorkMock.Setup(x => x.Projects).Returns(_projectRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(_taskRepositoryMock.Object);

        _handler = new GetTasksQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProjectExists_ShouldReturnPaginatedTasks()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateTestProject(projectId);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var tasks = CreateTestTasks(project, 5);
        _taskRepositoryMock
            .Setup(x => x.GetPagedAsync(
                projectId, 1, 10, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, 5));

        var query = new GetTasksQuery(projectId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var query = new GetTasksQuery(projectId, 1, 10);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var sprintId = Guid.NewGuid();
        var searchTerm = "bug";

        var project = CreateTestProject(projectId);
        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _taskRepositoryMock
            .Setup(x => x.GetPagedAsync(
                projectId, 1, 10, statusId, assigneeId, sprintId, searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskItem>(), 0));

        var query = new GetTasksQuery(projectId, 1, 10, statusId, assigneeId, sprintId, searchTerm);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _taskRepositoryMock.Verify(
            x => x.GetPagedAsync(projectId, 1, 10, statusId, assigneeId, sprintId, searchTerm, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = CreateTestProject(projectId);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _taskRepositoryMock
            .Setup(x => x.GetPagedAsync(
                projectId, 1, 10, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskItem>(), 0));

        var query = new GetTasksQuery(projectId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private Project CreateTestProject(Guid projectId)
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        typeof(Project).BaseType!.GetProperty("Id")!.SetValue(project, projectId);
        return project;
    }

    private List<TaskItem> CreateTestTasks(Project project, int count)
    {
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = User.Create(Email.Create("test@test.com").Value, "Test", Guid.NewGuid()).Value;
        
        project.AddStatus(status);
        
        return Enumerable.Range(1, count)
            .Select(i => TaskItem.Create(project, $"Task {i}", status, reporter, TaskType.Task, reporter.Id).Value)
            .ToList();
    }
}
