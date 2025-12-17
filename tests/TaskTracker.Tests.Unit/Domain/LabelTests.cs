using TaskTracker.Domain.Entities;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class LabelTests
{
    private Project CreateTestProject()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        return Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = Label.Create(project, "Bug", "#FF0000", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bug");
        result.Value.Color.Should().Be("#FF0000");
        result.Value.ProjectId.Should().Be(project.Id);
    }

    [Fact]
    public void Create_WithDescription_ShouldSetDescription()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = Label.Create(project, "Bug", "#FF0000", Guid.NewGuid(), description: "Represents a bug in the system");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("Represents a bug in the system");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = Label.Create(project, "", "#FF0000", Guid.NewGuid());

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
        var result = Label.Create(project, longName, "#FF0000", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("50");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateLabel()
    {
        // Arrange
        var label = CreateValidLabel();

        // Act
        var result = label.Update("Feature", "#00FF00", "Feature description", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        label.Name.Should().Be("Feature");
        label.Color.Should().Be("#00FF00");
        label.Description.Should().Be("Feature description");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var label = CreateValidLabel();

        // Act
        var result = label.Update("", "#00FF00", null, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var label = CreateValidLabel();
        var longName = new string('a', 51);

        // Act
        var result = label.Update(longName, "#00FF00", null, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    private Label CreateValidLabel()
    {
        var project = CreateTestProject();
        return Label.Create(project, "Bug", "#FF0000", Guid.NewGuid()).Value;
    }
}
