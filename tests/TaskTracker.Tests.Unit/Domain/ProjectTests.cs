using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Domain.Events;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class ProjectTests
{
    private Workspace CreateTestWorkspace()
    {
        var slug = Slug.Create("test-workspace").Value;
        return Workspace.Create("Test Workspace", slug, Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "My Project", slug, "PROJ", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Project");
        result.Value.Slug.Should().Be(slug);
        result.Value.Prefix.Should().Be("PROJ");
        result.Value.WorkspaceId.Should().Be(workspace.Id);
        result.Value.NextTaskNumber.Should().Be(1);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "", slug, "PROJ", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;
        var longName = new string('a', 101);

        // Act
        var result = Project.Create(workspace, longName, slug, "PROJ", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("100");
    }

    [Fact]
    public void Create_WithEmptyPrefix_ShouldReturnFailure()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "My Project", slug, "", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("prefix");
    }

    [Fact]
    public void Create_WithTooLongPrefix_ShouldReturnFailure()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "My Project", slug, "TOOLONGPREFIX", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("10");
    }

    [Fact]
    public void Create_WithInvalidPrefixCharacters_ShouldReturnFailure()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "My Project", slug, "PROJ-1", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("letters and digits");
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "My Project", slug, "PROJ", Guid.NewGuid());

        // Assert
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<ProjectCreatedEvent>();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProject()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = project.Update("Updated Name", "New description", "https://icon.url", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Name.Should().Be("Updated Name");
        project.Description.Should().Be("New description");
        project.IconUrl.Should().Be("https://icon.url");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = project.Update("", null, null, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void GetAndIncrementTaskNumber_ShouldIncrementAndReturn()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var first = project.GetAndIncrementTaskNumber();
        var second = project.GetAndIncrementTaskNumber();
        var third = project.GetAndIncrementTaskNumber();

        // Assert
        first.Should().Be(1);
        second.Should().Be(2);
        third.Should().Be(3);
    }

    [Fact]
    public void AddStatus_ShouldAddStatusToProject()
    {
        // Arrange
        var project = CreateTestProject();
        var status = WorkflowStatus.Create(project, "In Progress", StatusCategory.InProgress, 2, Guid.NewGuid()).Value;

        // Act
        project.AddStatus(status);

        // Assert
        project.Statuses.Should().HaveCount(1);
        project.Statuses.First().Name.Should().Be("In Progress");
    }

    [Fact]
    public void AddStatus_WithDefaultStatus_ShouldSetProjectDefaultStatus()
    {
        // Arrange
        var project = CreateTestProject();
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid(), isDefault: true).Value;

        // Act
        project.AddStatus(status);

        // Assert
        project.DefaultStatusId.Should().Be(status.Id);
        project.DefaultStatus.Should().Be(status);
    }

    [Fact]
    public void AddCustomField_ShouldAddFieldToProject()
    {
        // Arrange
        var project = CreateTestProject();
        var field = CustomFieldDefinition.Create(project, "Priority Level", CustomFieldType.Text, Guid.NewGuid()).Value;

        // Act
        project.AddCustomField(field);

        // Assert
        project.CustomFields.Should().HaveCount(1);
        project.CustomFields.First().Name.Should().Be("Priority Level");
    }

    [Fact]
    public void Create_ShouldConvertPrefixToUppercase()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("my-project").Value;

        // Act
        var result = Project.Create(workspace, "My Project", slug, "proj", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Prefix.Should().Be("PROJ");
    }

    private Project CreateTestProject()
    {
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("test-project").Value;
        return Project.Create(workspace, "Test Project", slug, "TEST", Guid.NewGuid()).Value;
    }
}
