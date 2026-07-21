namespace GrillBot.Core.Database;

public interface IQueryableModel<TEntity> where TEntity : class
{
    IQueryable<TEntity> SetQuery(IQueryable<TEntity> query);
    IQueryable<TEntity> SetIncludes(IQueryable<TEntity> query);
    IQueryable<TEntity> SetSort(IQueryable<TEntity> query);
}
