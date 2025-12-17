using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Label for categorizing tasks
/// </summary>
public class Label : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#808080";
    public string? Description { get; private set; }
    
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    private Label() { }

    public static Result<Label> Create(Project project, string name, string color, Guid createdBy, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Label>("Label name cannot be empty");

        if (name.Length > 50)
            return Result.Failure<Label>("Label name cannot exceed 50 characters");

        var label = new Label
        {
            ProjectId = project.Id,
            Project = project,
            Name = name.Trim(),
            Color = color,
            Description = description?.Trim()
        };

        label.SetCreated(createdBy);
        return Result.Success(label);
    }

    public Result Update(string name, string color, string? description, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Label name cannot be empty");

        if (name.Length > 50)
            return Result.Failure("Label name cannot exceed 50 characters");

        Name = name.Trim();
        Color = color;
        Description = description?.Trim();
        SetUpdated(updatedBy);

        return Result.Success();
    }
}
