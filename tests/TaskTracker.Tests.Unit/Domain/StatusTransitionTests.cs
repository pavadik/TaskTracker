using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class StatusTransitionTests
{
    private Project CreateTestProject()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        return Project.Create(workspace, "Test Project", Slug.Create("test-project").Value, "TEST", Guid.NewGuid()).Value;
    }

    private WorkflowStatus CreateStatus(Project project, string name, StatusCategory category, int order)
    {
        return WorkflowStatus.Create(project, name, category, order, Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var project = CreateTestProject();
        var fromStatus = CreateStatus(project, "To Do", StatusCategory.ToDo, 1);
        var toStatus = CreateStatus(project, "In Progress", StatusCategory.InProgress, 2);

        // Act
        var result = StatusTransition.Create(fromStatus, toStatus, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FromStatusId.Should().Be(fromStatus.Id);
        result.Value.ToStatusId.Should().Be(toStatus.Id);
    }

    [Fact]
    public void Create_WithAllOptionalParameters_ShouldSetThemCorrectly()
    {
        // Arrange
        var project = CreateTestProject();
        var fromStatus = CreateStatus(project, "To Do", StatusCategory.ToDo, 1);
        var toStatus = CreateStatus(project, "In Progress", StatusCategory.InProgress, 2);
        var autoAssignUserId = Guid.NewGuid();

        // Act
        var result = StatusTransition.Create(
            fromStatus, 
            toStatus, 
            Guid.NewGuid(),
            name: "Start Work",
            autoAssignUserId: autoAssignUserId,
            requiresComment: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Start Work");
        result.Value.AutoAssignUserId.Should().Be(autoAssignUserId);
        result.Value.RequiresComment.Should().BeTrue();
    }

    [Fact]
    public void Create_WithSameFromAndToStatus_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateStatus(project, "To Do", StatusCategory.ToDo, 1);

        // Act
        var result = StatusTransition.Create(status, status, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("same status");
    }

    [Fact]
    public void Create_WithStatusesFromDifferentProjects_ShouldReturnFailure()
    {
        // Arrange
        var project1 = CreateTestProject();
        var project2 = CreateTestProject();
        var fromStatus = CreateStatus(project1, "To Do", StatusCategory.ToDo, 1);
        var toStatus = CreateStatus(project2, "In Progress", StatusCategory.InProgress, 2);

        // Act
        var result = StatusTransition.Create(fromStatus, toStatus, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("same project");
    }

    [Fact]
    public void Update_ShouldUpdateTransitionProperties()
    {
        // Arrange
        var transition = CreateValidTransition();
        var newAutoAssignUserId = Guid.NewGuid();

        // Act
        transition.Update("Updated Name", newAutoAssignUserId, true, Guid.NewGuid());

        // Assert
        transition.Name.Should().Be("Updated Name");
        transition.AutoAssignUserId.Should().Be(newAutoAssignUserId);
        transition.RequiresComment.Should().BeTrue();
    }

    private StatusTransition CreateValidTransition()
    {
        var project = CreateTestProject();
        var fromStatus = CreateStatus(project, "To Do", StatusCategory.ToDo, 1);
        var toStatus = CreateStatus(project, "In Progress", StatusCategory.InProgress, 2);
        return StatusTransition.Create(fromStatus, toStatus, Guid.NewGuid()).Value;
    }
}
