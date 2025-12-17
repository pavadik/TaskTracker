using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class TaskLabelTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var (task, project) = CreateTestTaskAndProject();
        var label = CreateTestLabel(project);

        // Act
        var result = TaskLabel.Create(task, label);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TaskId.Should().Be(task.Id);
        result.Value.LabelId.Should().Be(label.Id);
    }

    [Fact]
    public void Create_ShouldSetNavigationProperties()
    {
        // Arrange
        var (task, project) = CreateTestTaskAndProject();
        var label = CreateTestLabel(project);

        // Act
        var result = TaskLabel.Create(task, label);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Task.Should().Be(task);
        result.Value.Label.Should().Be(label);
    }

    [Fact]
    public void Create_MultipleTimes_ShouldCreateMultipleAssociations()
    {
        // Arrange
        var (task, project) = CreateTestTaskAndProject();
        var label1 = CreateTestLabel(project, "Bug");
        var label2 = CreateTestLabel(project, "Feature");

        // Act
        var result1 = TaskLabel.Create(task, label1);
        var result2 = TaskLabel.Create(task, label2);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.LabelId.Should().NotBe(result2.Value.LabelId);
    }

    [Fact]
    public void Create_WithDifferentProjects_ShouldReturnFailure()
    {
        // Arrange
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        
        var project1 = Project.Create(workspace, "Project 1", Slug.Create("proj1").Value, "PRJ1", Guid.NewGuid()).Value;
        var project2 = Project.Create(workspace, "Project 2", Slug.Create("proj2").Value, "PRJ2", Guid.NewGuid()).Value;
        
        var status = WorkflowStatus.Create(project1, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = User.Create(Email.Create("test@test.com").Value, "Test", Guid.NewGuid()).Value;
        var task = TaskItem.Create(project1, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
        
        var labelFromDifferentProject = Label.Create(project2, "Bug", "#FF0000", Guid.NewGuid()).Value;

        // Act
        var result = TaskLabel.Create(task, labelFromDifferentProject);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("same project");
    }

    [Fact]
    public void Create_WithSameProject_ShouldSucceed()
    {
        // Arrange
        var (task, project) = CreateTestTaskAndProject();
        var label = Label.Create(project, "Same Project Label", "#00FF00", Guid.NewGuid()).Value;

        // Act
        var result = TaskLabel.Create(task, label);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Task.ProjectId.Should().Be(result.Value.Label.ProjectId);
    }

    [Fact]
    public void Create_WithMultipleLabels_ShouldAllHaveSameTaskId()
    {
        // Arrange
        var (task, project) = CreateTestTaskAndProject();
        var labels = new[]
        {
            CreateTestLabel(project, "Bug"),
            CreateTestLabel(project, "Feature"),
            CreateTestLabel(project, "Urgent")
        };

        // Act
        var results = labels.Select(l => TaskLabel.Create(task, l)).ToList();

        // Assert
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results.Select(r => r.Value.TaskId).Should().AllBeEquivalentTo(task.Id);
    }

    private (TaskItem task, Project project) CreateTestTaskAndProject()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = User.Create(Email.Create("test@test.com").Value, "Test", Guid.NewGuid()).Value;
        var task = TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
        return (task, project);
    }

    private Label CreateTestLabel(Project project, string name = "Bug")
    {
        return Label.Create(project, name, "#FF0000", Guid.NewGuid()).Value;
    }
}
