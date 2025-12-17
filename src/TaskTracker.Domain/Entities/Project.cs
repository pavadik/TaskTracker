using TaskTracker.Domain.Common;
using TaskTracker.Domain.Events;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Project entity - contains tasks, workflows, and settings
/// </summary>
public class Project : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string Prefix { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public int NextTaskNumber { get; private set; } = 1;
    
    public Guid WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    
    public Guid? DefaultStatusId { get; private set; }
    public WorkflowStatus? DefaultStatus { get; private set; }

    private readonly List<TaskItem> _tasks = new();
    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    private readonly List<WorkflowStatus> _statuses = new();
    public IReadOnlyCollection<WorkflowStatus> Statuses => _statuses.AsReadOnly();

    private readonly List<CustomFieldDefinition> _customFields = new();
    public IReadOnlyCollection<CustomFieldDefinition> CustomFields => _customFields.AsReadOnly();

    private readonly List<Sprint> _sprints = new();
    public IReadOnlyCollection<Sprint> Sprints => _sprints.AsReadOnly();

    private Project() { }

    public static Result<Project> Create(
        Workspace workspace, 
        string name, 
        Slug slug, 
        string prefix,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Project>("Project name cannot be empty");

        if (name.Length > 100)
            return Result.Failure<Project>("Project name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(prefix))
            return Result.Failure<Project>("Project prefix cannot be empty");

        if (prefix.Length > 10)
            return Result.Failure<Project>("Project prefix cannot exceed 10 characters");

        if (!prefix.All(char.IsLetterOrDigit))
            return Result.Failure<Project>("Project prefix can only contain letters and digits");

        var project = new Project
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Name = name.Trim(),
            Slug = slug,
            Prefix = prefix.ToUpperInvariant()
        };

        project.SetCreated(createdBy);
        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, workspace.Id, name, slug.Value, prefix));

        return Result.Success(project);
    }

    public Result Update(string name, string? description, string? iconUrl, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Project name cannot be empty");

        if (name.Length > 100)
            return Result.Failure("Project name cannot exceed 100 characters");

        Name = name.Trim();
        Description = description?.Trim();
        IconUrl = iconUrl?.Trim();
        SetUpdated(updatedBy);

        return Result.Success();
    }

    public int GetAndIncrementTaskNumber()
    {
        return NextTaskNumber++;
    }

    public void AddStatus(WorkflowStatus status)
    {
        _statuses.Add(status);
        if (DefaultStatusId is null && status.IsDefault)
        {
            DefaultStatusId = status.Id;
            DefaultStatus = status;
        }
    }

    public void SetDefaultStatus(WorkflowStatus status)
    {
        DefaultStatusId = status.Id;
        DefaultStatus = status;
    }

    public void AddCustomField(CustomFieldDefinition field)
    {
        _customFields.Add(field);
    }

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
    }

    public void AddSprint(Sprint sprint)
    {
        _sprints.Add(sprint);
    }
}
