using FluentValidation.TestHelper;
using TaskTracker.Application.Features.Tasks.Commands;
using TaskTracker.Application.Features.Tasks.Validators;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Tests.Unit.Application;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator;

    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: Guid.NewGuid(),
            Title: "Test Task",
            Description: null,
            Type: TaskType.Task);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: Guid.NewGuid(),
            Title: "",
            Description: null,
            Type: TaskType.Task);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyProjectId_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: Guid.Empty,
            Title: "Test Task",
            Description: null,
            Type: TaskType.Task);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Fact]
    public void Validate_WithTooLongTitle_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: Guid.NewGuid(),
            Title: new string('a', 501),
            Description: null,
            Type: TaskType.Task);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
