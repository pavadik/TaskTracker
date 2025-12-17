using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Domain.Events;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class TaskItemTests
{
    private Project CreateTestProject()
    {
        var workspace = CreateTestWorkspace();
        var slug = Slug.Create("test-project").Value;
        return Project.Create(workspace, "Test Project", slug, "TEST", Guid.NewGuid()).Value;
    }

    private Workspace CreateTestWorkspace()
    {
        var slug = Slug.Create("test-workspace").Value;
        return Workspace.Create("Test Workspace", slug, Guid.NewGuid()).Value;
    }

    private User CreateTestUser()
    {
        var email = Email.Create("test@example.com").Value;
        return User.Create(email, "Test User", Guid.NewGuid()).Value;
    }

    private WorkflowStatus CreateTestStatus(Project project, string name = "To Do", StatusCategory category = StatusCategory.ToDo, bool isDefault = true)
    {
        return WorkflowStatus.Create(project, name, category, 1, Guid.NewGuid(), null, "#999999", isDefault).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();

        // Act
        var result = TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Test Task");
        result.Value.FriendlyId.Should().NotBeNull();
        result.Value.ProjectId.Should().Be(project.Id);
        result.Value.StatusId.Should().Be(status.Id);
        result.Value.ReporterId.Should().Be(reporter.Id);
        result.Value.Type.Should().Be(TaskType.Task);
        result.Value.Priority.Should().Be(TaskPriority.None);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();

        // Act
        var result = TaskItem.Create(project, "", status, reporter, TaskType.Task, reporter.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("title");
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();
        var longTitle = new string('a', 501);

        // Act
        var result = TaskItem.Create(project, longTitle, status, reporter, TaskType.Task, reporter.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("500");
    }

    [Fact]
    public void Create_WithAllOptionalFields_ShouldSetThemCorrectly()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();
        var assignee = CreateTestUser();

        // Act
        var result = TaskItem.Create(
            project, "Test Task", status, reporter, TaskType.Bug, reporter.Id,
            description: "Test description", priority: TaskPriority.High, assignee: assignee);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("Test description");
        result.Value.Type.Should().Be(TaskType.Bug);
        result.Value.Priority.Should().Be(TaskPriority.High);
        result.Value.AssigneeId.Should().Be(assignee.Id);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();

        // Act
        var result = TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id);

        // Assert
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<TaskCreatedEvent>();
    }

    [Fact]
    public void Create_ShouldIncrementProjectTaskNumber()
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();

        // Act
        var task1 = TaskItem.Create(project, "Task 1", status, reporter, TaskType.Task, reporter.Id).Value;
        var task2 = TaskItem.Create(project, "Task 2", status, reporter, TaskType.Task, reporter.Id).Value;
        var task3 = TaskItem.Create(project, "Task 3", status, reporter, TaskType.Task, reporter.Id).Value;

        // Assert
        task1.FriendlyId.Value.Should().Be("TEST-1");
        task2.FriendlyId.Value.Should().Be("TEST-2");
        task3.FriendlyId.Value.Should().Be("TEST-3");
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateAndRecordHistory()
    {
        // Arrange
        var task = CreateValidTask();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.UpdateTitle("New Title", updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Title.Should().Be("New Title");
        task.History.Should().HaveCount(1);
        task.History.First().FieldName.Should().Be("Title");
    }

    [Fact]
    public void UpdateTitle_WithEmptyTitle_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateValidTask();

        // Act
        var result = task.UpdateTitle("", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateAndRecordHistory()
    {
        // Arrange
        var task = CreateValidTask();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.UpdateDescription("New description", updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Description.Should().Be("New description");
        task.History.Should().Contain(h => h.FieldName == "Description");
    }

    [Fact]
    public void ChangePriority_ShouldUpdateAndRecordHistory()
    {
        // Arrange
        var task = CreateValidTask();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.ChangePriority(TaskPriority.Critical, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Priority.Should().Be(TaskPriority.Critical);
        task.History.Should().Contain(h => h.FieldName == "Priority");
    }

    [Fact]
    public void SetDueDate_ShouldUpdateAndRecordHistory()
    {
        // Arrange
        var task = CreateValidTask();
        var dueDate = DateTime.UtcNow.AddDays(14);
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.SetDueDate(dueDate, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.DueDate.Should().Be(dueDate);
        task.History.Should().Contain(h => h.FieldName == "DueDate");
    }

    [Fact]
    public void SetStoryPoints_WithValidValue_ShouldUpdate()
    {
        // Arrange
        var task = CreateValidTask();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.SetStoryPoints(8, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.StoryPoints.Should().Be(8);
    }

    [Fact]
    public void SetStoryPoints_WithNegativeValue_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateValidTask();

        // Act
        var result = task.SetStoryPoints(-1, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Assign_ShouldUpdateAssigneeAndRecordHistory()
    {
        // Arrange
        var task = CreateValidTask();
        var assignee = CreateTestUser();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.Assign(assignee, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.AssigneeId.Should().Be(assignee.Id);
        task.Assignee.Should().Be(assignee);
        task.History.Should().Contain(h => h.FieldName == "AssigneeId");
    }

    [Fact]
    public void Unassign_ShouldClearAssignee()
    {
        // Arrange
        var task = CreateValidTask();
        var assignee = CreateTestUser();
        task.Assign(assignee, Guid.NewGuid());
        var updatedBy = Guid.NewGuid();

        // Act
        var result = task.Assign(null, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.AssigneeId.Should().BeNull();
        task.Assignee.Should().BeNull();
    }

    [Theory]
    [InlineData(TaskType.Task)]
    [InlineData(TaskType.Bug)]
    [InlineData(TaskType.Story)]
    [InlineData(TaskType.Epic)]
    [InlineData(TaskType.Subtask)]
    public void Create_WithAllTaskTypes_ShouldSucceed(TaskType taskType)
    {
        // Arrange
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();

        // Act
        var result = TaskItem.Create(project, "Test Task", status, reporter, taskType, reporter.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(taskType);
    }

    [Theory]
    [InlineData(TaskPriority.None)]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public void ChangePriority_WithAllPriorities_ShouldSucceed(TaskPriority priority)
    {
        // Arrange
        var task = CreateValidTask();

        // Act
        var result = task.ChangePriority(priority, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Priority.Should().Be(priority);
    }

    private TaskItem CreateValidTask()
    {
        var project = CreateTestProject();
        var status = CreateTestStatus(project);
        var reporter = CreateTestUser();
        return TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
    }
}
