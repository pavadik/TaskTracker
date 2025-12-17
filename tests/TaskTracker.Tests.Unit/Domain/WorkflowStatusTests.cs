using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class WorkflowStatusTests
{
    private Project CreateTestProject()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        return Project.Create(workspace, "Test Project", Slug.Create("test-project").Value, "TEST", Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("To Do");
        result.Value.Category.Should().Be(StatusCategory.ToDo);
        result.Value.Order.Should().Be(1);
        result.Value.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void Create_WithAllParameters_ShouldSetThemCorrectly()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = WorkflowStatus.Create(
            project, 
            "In Progress", 
            StatusCategory.InProgress, 
            2, 
            Guid.NewGuid(),
            description: "Work in progress",
            color: "#0000FF",
            isDefault: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("Work in progress");
        result.Value.Color.Should().Be("#0000FF");
        result.Value.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = WorkflowStatus.Create(project, "", StatusCategory.ToDo, 1, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var longName = new string('a', 51);

        // Act
        var result = WorkflowStatus.Create(project, longName, StatusCategory.ToDo, 1, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("50");
    }

    [Theory]
    [InlineData(StatusCategory.ToDo)]
    [InlineData(StatusCategory.InProgress)]
    [InlineData(StatusCategory.Done)]
    [InlineData(StatusCategory.Cancelled)]
    public void Create_WithAllCategories_ShouldSucceed(StatusCategory category)
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = WorkflowStatus.Create(project, "Status", category, 1, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(category);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        var status = CreateValidStatus();

        // Act
        var result = status.Update("Updated Status", "New description", "#0000FF", StatusCategory.InProgress, 2, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        status.Name.Should().Be("Updated Status");
        status.Description.Should().Be("New description");
        status.Color.Should().Be("#0000FF");
        status.Category.Should().Be(StatusCategory.InProgress);
        status.Order.Should().Be(2);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var status = CreateValidStatus();

        // Act
        var result = status.Update("", null, "#999999", StatusCategory.ToDo, 1, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SetAsDefault_ShouldMakeStatusDefault()
    {
        // Arrange
        var status = CreateValidStatus();
        status.IsDefault.Should().BeFalse();

        // Act
        status.SetAsDefault();

        // Assert
        status.IsDefault.Should().BeTrue();
    }

    private WorkflowStatus CreateValidStatus()
    {
        var project = CreateTestProject();
        return WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
    }
}
