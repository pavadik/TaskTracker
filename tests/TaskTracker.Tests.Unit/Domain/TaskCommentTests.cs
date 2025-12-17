using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class TaskCommentTests
{
    private TaskItem CreateTestTask()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = User.Create(Email.Create("test@test.com").Value, "Test", Guid.NewGuid()).Value;
        return TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
    }

    private User CreateTestUser()
    {
        return User.Create(Email.Create("author@test.com").Value, "Author", Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var task = CreateTestTask();
        var author = CreateTestUser();

        // Act
        var result = TaskComment.Create(task, author, "This is a comment", author.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("This is a comment");
        result.Value.AuthorId.Should().Be(author.Id);
        result.Value.TaskId.Should().Be(task.Id);
    }

    [Fact]
    public void Create_WithEmptyContent_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var author = CreateTestUser();

        // Act
        var result = TaskComment.Create(task, author, "", author.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("content");
    }

    [Fact]
    public void Create_WithWhitespaceContent_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var author = CreateTestUser();

        // Act
        var result = TaskComment.Create(task, author, "   ", author.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithTooLongContent_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var author = CreateTestUser();
        var longContent = new string('a', 10001);

        // Act
        var result = TaskComment.Create(task, author, longContent, author.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("10000");
    }

    [Fact]
    public void Create_WithMaxLengthContent_ShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();
        var author = CreateTestUser();
        var maxContent = new string('a', 10000);

        // Act
        var result = TaskComment.Create(task, author, maxContent, author.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Update_WithValidContent_ShouldUpdateComment()
    {
        // Arrange
        var comment = CreateValidComment();

        // Act
        var result = comment.Update("Updated content", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be("Updated content");
    }

    [Fact]
    public void Update_WithEmptyContent_ShouldReturnFailure()
    {
        // Arrange
        var comment = CreateValidComment();

        // Act
        var result = comment.Update("", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_WithTooLongContent_ShouldReturnFailure()
    {
        // Arrange
        var comment = CreateValidComment();
        var longContent = new string('a', 10001);

        // Act
        var result = comment.Update(longContent, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldTrimContent()
    {
        // Arrange
        var task = CreateTestTask();
        var author = CreateTestUser();

        // Act
        var result = TaskComment.Create(task, author, "  Comment with spaces  ", author.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Comment with spaces");
    }

    private TaskComment CreateValidComment()
    {
        var task = CreateTestTask();
        var author = CreateTestUser();
        return TaskComment.Create(task, author, "Original comment", author.Id).Value;
    }
}
