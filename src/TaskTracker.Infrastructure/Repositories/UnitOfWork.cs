using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Repositories;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaskTrackerDbContext _context;
    private readonly IMediator _mediator;
    private IDbContextTransaction? _currentTransaction;

    private IUserRepository? _users;
    private IWorkspaceRepository? _workspaces;
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;

    public UnitOfWork(TaskTrackerDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IWorkspaceRepository Workspaces => _workspaces ??= new WorkspaceRepository(_context);
    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await (_currentTransaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await (_currentTransaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
