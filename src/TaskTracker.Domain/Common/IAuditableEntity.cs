namespace TaskTracker.Domain.Common;

/// <summary>
/// Interface for audit tracking
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    Guid CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
    
    void SetCreated(Guid userId, DateTime? timestamp = null);
    void SetUpdated(Guid userId, DateTime? timestamp = null);
}
