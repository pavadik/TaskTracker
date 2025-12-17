using FluentValidation;
using TaskTracker.Application.Features.Tasks.Commands;

namespace TaskTracker.Application.Features.Tasks.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(50000)
            .WithMessage("Description cannot exceed 50000 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid task type");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid priority");

        RuleFor(x => x.StoryPoints)
            .GreaterThanOrEqualTo(0)
            .When(x => x.StoryPoints.HasValue)
            .WithMessage("Story points must be non-negative");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date cannot be in the past");
    }
}
