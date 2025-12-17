using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class TaskAttachmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "document.pdf", 
            "/files/document.pdf", 
            "application/pdf", 
            1024,
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("document.pdf");
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.StoragePath.Should().Be("/files/document.pdf");
        result.Value.FileSize.Should().Be(1024);
    }

    [Fact]
    public void Create_WithEmptyFileName_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "", 
            "/files/doc.pdf", 
            "application/pdf", 
            1024, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithWhitespaceFileName_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "   ", 
            "/files/doc.pdf", 
            "application/pdf", 
            1024, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyStoragePath_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "doc.pdf", 
            "", 
            "application/pdf", 
            1024, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithZeroFileSize_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "doc.pdf", 
            "/files/doc.pdf", 
            "application/pdf", 
            0, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNegativeFileSize_ShouldReturnFailure()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "doc.pdf", 
            "/files/doc.pdf", 
            "application/pdf", 
            -100, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldTrimFileNameAndPath()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "  doc.pdf  ", 
            "  /files/doc.pdf  ", 
            "application/pdf", 
            1024, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("doc.pdf");
        result.Value.StoragePath.Should().Be("/files/doc.pdf");
    }

    [Fact]
    public void Create_ShouldSetTaskAndUploaderRelations()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "doc.pdf", 
            "/files/doc.pdf", 
            "application/pdf", 
            2048, 
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TaskId.Should().Be(task.Id);
        result.Value.Task.Should().Be(task);
        result.Value.UploadedById.Should().Be(uploader.Id);
        result.Value.UploadedBy.Should().Be(uploader);
    }

    [Fact]
    public void Create_WithDifferentContentTypes_ShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act & Assert
        var pdfResult = TaskAttachment.Create(task, uploader, "doc.pdf", "/files/doc.pdf", "application/pdf", 1024, uploader.Id);
        var imageResult = TaskAttachment.Create(task, uploader, "image.png", "/files/image.png", "image/png", 2048, uploader.Id);
        var textResult = TaskAttachment.Create(task, uploader, "readme.txt", "/files/readme.txt", "text/plain", 512, uploader.Id);

        pdfResult.IsSuccess.Should().BeTrue();
        imageResult.IsSuccess.Should().BeTrue();
        textResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLargeFileSize_ShouldSucceed()
    {
        // Arrange
        var task = CreateTestTask();
        var uploader = CreateTestUser();

        // Act
        var result = TaskAttachment.Create(
            task, 
            uploader, 
            "large_file.zip", 
            "/files/large_file.zip", 
            "application/zip", 
            10_000_000_000L, // 10GB
            uploader.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileSize.Should().Be(10_000_000_000L);
    }

    private TaskItem CreateTestTask()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        var project = Project.Create(workspace, "Test", Slug.Create("test").Value, "TEST", Guid.NewGuid()).Value;
        var status = WorkflowStatus.Create(project, "To Do", StatusCategory.ToDo, 1, Guid.NewGuid()).Value;
        var reporter = CreateTestUser();
        return TaskItem.Create(project, "Test Task", status, reporter, TaskType.Task, reporter.Id).Value;
    }

    private User CreateTestUser()
    {
        return User.Create(Email.Create("test@test.com").Value, "Test User", Guid.NewGuid()).Value;
    }
}
