namespace TaskTracker.Domain.Common;

/// <summary>
/// Base class for auditable entities combining Entity with audit fields
/// </summary>
public abstract class AuditableEntity : Entity, IAuditableEntity, ISoftDeletable
{
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    
    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    protected AuditableEntity() : base() { }
    
    protected AuditableEntity(Guid id) : base(id) { }

    public void SetCreated(Guid userId, DateTime? timestamp = null)
    {
        CreatedAt = timestamp ?? DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void SetUpdated(Guid userId, DateTime? timestamp = null)
    {
        UpdatedAt = timestamp ?? DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void MarkAsDeleted(Guid deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
