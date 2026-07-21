using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Core.Database.Repository;

public abstract class SubRepositoryBase<TContext>(TContext _context, ICounterManager _counterManager) where TContext : DbContext
{
    protected TContext DbContext => _context;

    protected IQueryable<TEntity> CreateQuery<TEntity>(IQueryableModel<TEntity> parameters, bool disableTracking = false,
        bool splitQuery = false) where TEntity : class
    {
        var query = _context.Set<TEntity>().AsQueryable();

        query = parameters.SetIncludes(query);
        query = parameters.SetQuery(query);
        query = parameters.SetSort(query);

        if (disableTracking)
            query = query.AsNoTracking();
        if (splitQuery)
            query = query.AsSplitQuery();

        return query;
    }

    protected CounterItem CreateCounter()
        => _counterManager.Create($"Repository.{GetType().Name.Replace("Repository", "")}");
}
