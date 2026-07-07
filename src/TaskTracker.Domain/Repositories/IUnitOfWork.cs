namespace TaskTracker.Domain.Repositories;

/// <summary>
/// Unit of Work interface for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IWorkspaceRepository Workspaces { get; }
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly marks an entity reachable from a tracked aggregate as newly added.
    /// Needed when the entity's PK is pre-set in its constructor (Guid.NewGuid()),
    /// so the persistence layer cannot infer Added state from the navigation alone.
    /// In-memory implementations may treat this as a no-op.
    /// </summary>
    void MarkAdded<TEntity>(TEntity entity) where TEntity : class;
}
