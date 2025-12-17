using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class TaskHistoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var task = CreateTestTask();
        var changedBy = Guid.NewGuid();

        // Act
        var result = TaskHistory.Create(task, "Title", "Old Title", "New Title", changedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FieldName.Should().Be("Title");
        result.Value.OldValue.Should().Be("Old Title");
        result.Value.NewValue.Should().Be("New Title");
        result.Value.ChangedById.Should().Be(changedBy);
    }

    [Fact]
    public void Create_WithEmptyFieldName_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();

        // Act
        var result = TaskHistory.Create(task, "", "Old", "New", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullOldValue_ShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();

        // Act
        var result = TaskHistory.Create(task, "Description", "", "New description", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullNewValue_ShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();

        // Act
        var result = TaskHistory.Create(task, "Description", "Old description", "", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private TaskItem CreateTestTask()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = User.Create(Email.Create("test@test.com").Value, "Test", Guid.NewGuid()).Value;
        return TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
    }
}
