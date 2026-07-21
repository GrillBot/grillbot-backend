using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Core.Database.Repository;

public abstract class RepositoryBase<TContext>(TContext _context, ICounterManager _counterManager) : IDisposable where TContext : DbContext
{
    private bool disposedValue;

    protected ICounterManager CounterManager => _counterManager;
    protected TContext DbContext => _context;

    public Task AddAsync<TEntity>(TEntity entity) where TEntity : class
        => _context.Set<TEntity>().AddAsync(entity).AsTask();

    public Task AddCollectionAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        => _context.Set<TEntity>().AddRangeAsync(entities);

    public void Remove<TEntity>(TEntity entity) where TEntity : class
        => _context.Set<TEntity>().Remove(entity);

    public void RemoveCollection<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        => _context.Set<TEntity>().RemoveRange(entities);

    public async Task<int> CommitAsync()
    {
        using (_counterManager.Create("Repository.Commit"))
            return await _context.SaveChangesAsync();
    }

    public bool IsPendingMigrations()
    {
        using (_counterManager.Create("Repository.Migrations"))
            return _context.Database.GetPendingMigrations().Any();
    }

    public async Task<bool> IsPendingMigrationsAsync()
    {
        using (_counterManager.Create("Repository.Migrations"))
            return (await _context.Database.GetPendingMigrationsAsync()).Any();
    }

    protected virtual void DisposeInternal() { }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _context.Dispose();
                DisposeInternal();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
