using FluentAssertions;
using Moq;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Features.Tasks.Commands;
using TaskTracker.Application.Features.Tasks.Handlers;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Tests.Unit.Application;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();

        _unitOfWorkMock.Setup(x => x.Projects).Returns(_projectRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(_taskRepositoryMock.Object);

        _handler = new CreateTaskCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var user = User.Create(Email.Create("test@test.com").Value, "Test User", userId).Value;
        SetEntityId(user, userId);

        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test Project", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        SetEntityId(project, projectId);
        
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid(), isDefault: true).Value;
        project.AddStatus(status);

        _projectRepositoryMock
            .Setup(x => x.GetWithStatusesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new CreateTaskCommand(
            projectId, "New Task", "Description", TaskType.Task, TaskPriority.Medium, null, null, null, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.FriendlyId.Should().Be("TEST-1");
        
        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new CreateTaskCommand(Guid.NewGuid(), "Task", null, TaskType.Task, TaskPriority.None, null, null, null, null, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var projectId = Guid.NewGuid();
        _projectRepositoryMock
            .Setup(x => x.GetWithStatusesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var command = new CreateTaskCommand(projectId, "Task", null, TaskType.Task, TaskPriority.None, null, null, null, null, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProjectHasNoStatuses_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var user = User.Create(Email.Create("test@test.com").Value, "Test User", userId).Value;
        SetEntityId(user, userId);

        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test Project", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        SetEntityId(project, projectId);
        // No statuses added

        _projectRepositoryMock
            .Setup(x => x.GetWithStatusesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new CreateTaskCommand(projectId, "Task", null, TaskType.Task, TaskPriority.None, null, null, null, null, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*workflow statuses*");
    }

    [Fact]
    public async Task Handle_WithAssignee_ShouldSetAssignee()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var reporter = User.Create(Email.Create("reporter@test.com").Value, "Reporter", userId).Value;
        SetEntityId(reporter, userId);
        
        var assignee = User.Create(Email.Create("assignee@test.com").Value, "Assignee", assigneeId).Value;
        SetEntityId(assignee, assigneeId);

        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test Project", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        SetEntityId(project, projectId);
        
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid(), isDefault: true).Value;
        project.AddStatus(status);

        _projectRepositoryMock
            .Setup(x => x.GetWithStatusesAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reporter);
            
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(assigneeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignee);

        var command = new CreateTaskCommand(
            projectId, "Task", null, TaskType.Task, TaskPriority.None, assigneeId, null, null, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AssigneeId.Should().Be(assigneeId);
    }

    private void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var type = typeof(T);
        while (type != null && type != typeof(object))
        {
            var idProp = type.GetProperty("Id");
            if (idProp != null && idProp.CanWrite)
            {
                idProp.SetValue(entity, id);
                return;
            }
            type = type.BaseType;
        }
    }
}
