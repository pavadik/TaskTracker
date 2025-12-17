using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Custom field definition for a project
/// </summary>
public class CustomFieldDefinition : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CustomFieldType FieldType { get; private set; }
    public bool IsRequired { get; private set; }
    public int Order { get; private set; }
    
    /// <summary>
    /// JSON-serialized options for select fields
    /// </summary>
    public string? Options { get; private set; }
    
    /// <summary>
    /// JSON-serialized default value
    /// </summary>
    public string? DefaultValue { get; private set; }
    
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    private CustomFieldDefinition() { }

    public static Result<CustomFieldDefinition> Create(
        Project project,
        string name,
        CustomFieldType fieldType,
        Guid createdBy,
        string? description = null,
        bool isRequired = false,
        int order = 0,
        string? options = null,
        string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CustomFieldDefinition>("Field name cannot be empty");

        if (name.Length > 100)
            return Result.Failure<CustomFieldDefinition>("Field name cannot exceed 100 characters");

        if ((fieldType == CustomFieldType.SingleSelect || fieldType == CustomFieldType.MultiSelect) 
            && string.IsNullOrWhiteSpace(options))
            return Result.Failure<CustomFieldDefinition>("Select fields require options");

        var field = new CustomFieldDefinition
        {
            ProjectId = project.Id,
            Project = project,
            Name = name.Trim(),
            Description = description?.Trim(),
            FieldType = fieldType,
            IsRequired = isRequired,
            Order = order,
            Options = options,
            DefaultValue = defaultValue
        };

        field.SetCreated(createdBy);
        return Result.Success(field);
    }

    public Result Update(
        string name,
        string? description,
        bool isRequired,
        int order,
        string? options,
        string? defaultValue,
        Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Field name cannot be empty");

        if (name.Length > 100)
            return Result.Failure("Field name cannot exceed 100 characters");

        Name = name.Trim();
        Description = description?.Trim();
        IsRequired = isRequired;
        Order = order;
        Options = options;
        DefaultValue = defaultValue;
        SetUpdated(updatedBy);

        return Result.Success();
    }
}
