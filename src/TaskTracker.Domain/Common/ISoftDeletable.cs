namespace TaskTracker.Domain.Common;

/// <summary>
/// Interface for entities that support soft delete
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    Guid? DeletedBy { get; }
    
    void MarkAsDeleted(Guid deletedBy);
    void Restore();
}
