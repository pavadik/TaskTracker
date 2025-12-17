using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// Task attachment entity
/// </summary>
public class TaskAttachment : AuditableEntity
{
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    
    public Guid TaskId { get; private set; }
    public TaskItem Task { get; private set; } = null!;
    
    public Guid UploadedById { get; private set; }
    public User UploadedBy { get; private set; } = null!;

    private TaskAttachment() { }

    public static Result<TaskAttachment> Create(
        TaskItem task, 
        User uploadedBy, 
        string fileName, 
        string storagePath,
        string contentType,
        long fileSize,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<TaskAttachment>("File name cannot be empty");

        if (string.IsNullOrWhiteSpace(storagePath))
            return Result.Failure<TaskAttachment>("Storage path cannot be empty");

        if (fileSize <= 0)
            return Result.Failure<TaskAttachment>("File size must be positive");

        var attachment = new TaskAttachment
        {
            TaskId = task.Id,
            Task = task,
            UploadedById = uploadedBy.Id,
            UploadedBy = uploadedBy,
            FileName = fileName.Trim(),
            StoragePath = storagePath.Trim(),
            ContentType = contentType,
            FileSize = fileSize
        };

        attachment.SetCreated(createdBy);
        return Result.Success(attachment);
    }
}
