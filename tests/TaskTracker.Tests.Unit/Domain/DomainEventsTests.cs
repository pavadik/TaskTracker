using TaskTracker.Domain.Events;
using TaskTracker.Domain.Enums;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class DomainEventsTests
{
    #region UserCreatedEvent Tests

    [Fact]
    public void UserCreatedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var displayName = "Test User";
        
        var @event = new UserCreatedEvent(userId, email, displayName);

        // Assert
        @event.UserId.Should().Be(userId);
        @event.Email.Should().Be(email);
        @event.DisplayName.Should().Be(displayName);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserCreatedEvent_MultipleInstances_ShouldHaveDifferentOccurredOn()
    {
        // Arrange & Act
        var evt1 = new UserCreatedEvent(Guid.NewGuid(), "user1@test.com", "User 1");
        Thread.Sleep(50);
        var evt2 = new UserCreatedEvent(Guid.NewGuid(), "user2@test.com", "User 2");

        // Assert
        evt1.OccurredOn.Should().BeBefore(evt2.OccurredOn);
    }

    #endregion

    #region WorkspaceCreatedEvent Tests

    [Fact]
    public void WorkspaceCreatedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var workspaceId = Guid.NewGuid();
        var name = "My Workspace";
        var slug = "my-workspace";
        var ownerId = Guid.NewGuid();
        
        var @event = new WorkspaceCreatedEvent(workspaceId, name, slug, ownerId);

        // Assert
        @event.WorkspaceId.Should().Be(workspaceId);
        @event.Name.Should().Be(name);
        @event.Slug.Should().Be(slug);
        @event.OwnerId.Should().Be(ownerId);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region ProjectCreatedEvent Tests

    [Fact]
    public void ProjectCreatedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var projectId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var name = "My Project";
        var slug = "my-project";
        var prefix = "PROJ";
        
        var @event = new ProjectCreatedEvent(projectId, workspaceId, name, slug, prefix);

        // Assert
        @event.ProjectId.Should().Be(projectId);
        @event.WorkspaceId.Should().Be(workspaceId);
        @event.Name.Should().Be(name);
        @event.Slug.Should().Be(slug);
        @event.Prefix.Should().Be(prefix);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region TaskCreatedEvent Tests

    [Fact]
    public void TaskCreatedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var taskId = Guid.NewGuid();
        var friendlyId = "PROJ-123";
        var projectId = Guid.NewGuid();
        var title = "New Task";
        var taskType = TaskType.Bug;
        var statusId = Guid.NewGuid();
        
        var @event = new TaskCreatedEvent(taskId, friendlyId, projectId, title, taskType, statusId);

        // Assert
        @event.TaskId.Should().Be(taskId);
        @event.FriendlyId.Should().Be(friendlyId);
        @event.ProjectId.Should().Be(projectId);
        @event.Title.Should().Be(title);
        @event.Type.Should().Be(taskType);
        @event.StatusId.Should().Be(statusId);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TaskCreatedEvent_WithDifferentTaskTypes_ShouldPreserveType()
    {
        // Arrange & Act
        var bugEvent = new TaskCreatedEvent(Guid.NewGuid(), "BUG-1", Guid.NewGuid(), "Bug", TaskType.Bug, Guid.NewGuid());
        var storyEvent = new TaskCreatedEvent(Guid.NewGuid(), "STORY-1", Guid.NewGuid(), "Story", TaskType.Story, Guid.NewGuid());
        var epicEvent = new TaskCreatedEvent(Guid.NewGuid(), "EPIC-1", Guid.NewGuid(), "Epic", TaskType.Epic, Guid.NewGuid());

        // Assert
        bugEvent.Type.Should().Be(TaskType.Bug);
        storyEvent.Type.Should().Be(TaskType.Story);
        epicEvent.Type.Should().Be(TaskType.Epic);
    }

    #endregion

    #region TaskUpdatedEvent Tests

    [Fact]
    public void TaskUpdatedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var taskId = Guid.NewGuid();
        var friendlyId = "PROJ-123";
        var projectId = Guid.NewGuid();
        var fieldName = "Title";
        var oldValue = "Old Title";
        var newValue = "New Title";
        
        var @event = new TaskUpdatedEvent(taskId, friendlyId, projectId, fieldName, oldValue, newValue);

        // Assert
        @event.TaskId.Should().Be(taskId);
        @event.FriendlyId.Should().Be(friendlyId);
        @event.ProjectId.Should().Be(projectId);
        @event.FieldName.Should().Be(fieldName);
        @event.OldValue.Should().Be(oldValue);
        @event.NewValue.Should().Be(newValue);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TaskUpdatedEvent_WithDifferentFields_ShouldPreserveData()
    {
        // Arrange & Act
        var titleEvent = new TaskUpdatedEvent(Guid.NewGuid(), "PROJ-1", Guid.NewGuid(), "Title", "Old", "New");
        var descEvent = new TaskUpdatedEvent(Guid.NewGuid(), "PROJ-2", Guid.NewGuid(), "Description", "Old Desc", "New Desc");
        var priorityEvent = new TaskUpdatedEvent(Guid.NewGuid(), "PROJ-3", Guid.NewGuid(), "Priority", "Low", "High");

        // Assert
        titleEvent.FieldName.Should().Be("Title");
        descEvent.FieldName.Should().Be("Description");
        priorityEvent.FieldName.Should().Be("Priority");
    }

    #endregion

    #region TaskStatusChangedEvent Tests

    [Fact]
    public void TaskStatusChangedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var taskId = Guid.NewGuid();
        var friendlyId = "PROJ-123";
        var projectId = Guid.NewGuid();
        var oldStatusId = Guid.NewGuid();
        var oldStatusName = "To Do";
        var newStatusId = Guid.NewGuid();
        var newStatusName = "In Progress";
        
        var @event = new TaskStatusChangedEvent(
            taskId, friendlyId, projectId, 
            oldStatusId, oldStatusName, 
            newStatusId, newStatusName);

        // Assert
        @event.TaskId.Should().Be(taskId);
        @event.FriendlyId.Should().Be(friendlyId);
        @event.ProjectId.Should().Be(projectId);
        @event.OldStatusId.Should().Be(oldStatusId);
        @event.OldStatusName.Should().Be(oldStatusName);
        @event.NewStatusId.Should().Be(newStatusId);
        @event.NewStatusName.Should().Be(newStatusName);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TaskStatusChangedEvent_WithStatusTransition_ShouldCaptureBothStates()
    {
        // Arrange
        var oldId = Guid.NewGuid();
        var newId = Guid.NewGuid();

        // Act
        var @event = new TaskStatusChangedEvent(
            Guid.NewGuid(), "TEST-1", Guid.NewGuid(),
            oldId, "Backlog",
            newId, "Done");

        // Assert
        @event.OldStatusId.Should().NotBe(@event.NewStatusId);
        @event.OldStatusName.Should().NotBe(@event.NewStatusName);
    }

    #endregion

    #region TaskAssignedEvent Tests

    [Fact]
    public void TaskAssignedEvent_ShouldContainCorrectData()
    {
        // Arrange & Act
        var taskId = Guid.NewGuid();
        var friendlyId = "PROJ-123";
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var assigneeName = "John Doe";
        
        var @event = new TaskAssignedEvent(taskId, friendlyId, projectId, assigneeId, assigneeName);

        // Assert
        @event.TaskId.Should().Be(taskId);
        @event.FriendlyId.Should().Be(friendlyId);
        @event.ProjectId.Should().Be(projectId);
        @event.AssigneeId.Should().Be(assigneeId);
        @event.AssigneeName.Should().Be(assigneeName);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TaskAssignedEvent_WithNullAssignee_ShouldWork()
    {
        // Arrange & Act
        var @event = new TaskAssignedEvent(Guid.NewGuid(), "PROJ-1", Guid.NewGuid(), null, null);

        // Assert
        @event.AssigneeId.Should().BeNull();
        @event.AssigneeName.Should().BeNull();
    }

    [Fact]
    public void TaskAssignedEvent_Unassignment_ShouldHaveNullAssignee()
    {
        // Arrange & Act
        var taskId = Guid.NewGuid();
        var @event = new TaskAssignedEvent(taskId, "TEST-1", Guid.NewGuid(), null, null);

        // Assert - unassignment is represented by null assignee
        @event.TaskId.Should().Be(taskId);
        @event.AssigneeId.Should().BeNull();
        @event.AssigneeName.Should().BeNull();
    }

    #endregion

    #region Event Ordering Tests

    [Fact]
    public void DomainEvents_CreatedSequentially_ShouldHaveChronologicalOrder()
    {
        // Arrange & Act
        var evt1 = new UserCreatedEvent(Guid.NewGuid(), "u1@test.com", "User 1");
        Thread.Sleep(10);
        var evt2 = new WorkspaceCreatedEvent(Guid.NewGuid(), "WS", "ws", Guid.NewGuid());
        Thread.Sleep(10);
        var evt3 = new ProjectCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Proj", "proj", "PRJ");
        Thread.Sleep(10);
        var evt4 = new TaskCreatedEvent(Guid.NewGuid(), "PRJ-1", Guid.NewGuid(), "Task", TaskType.Task, Guid.NewGuid());

        // Assert
        evt1.OccurredOn.Should().BeBefore(evt2.OccurredOn);
        evt2.OccurredOn.Should().BeBefore(evt3.OccurredOn);
        evt3.OccurredOn.Should().BeBefore(evt4.OccurredOn);
    }

    #endregion
}
