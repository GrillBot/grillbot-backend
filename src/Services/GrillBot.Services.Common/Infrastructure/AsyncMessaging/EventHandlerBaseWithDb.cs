using GrillBot.Services.Common.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Services.Common.Infrastructure.AsyncMessaging;

/// <summary>
/// <see cref="EventHandlerBase"/> plus the service's database, wrapped in the same
/// <see cref="ContextHelper{TDbContext}"/> the API actions use so query timings land on the
/// same counters.
/// </summary>
public abstract class EventHandlerBaseWithDb<TDbContext> : EventHandlerBase where TDbContext : DbContext
{
    protected TDbContext DbContext => ContextHelper.DbContext;
    protected ContextHelper<TDbContext> ContextHelper { get; }

    protected EventHandlerBaseWithDb(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<TDbContext>();
        ContextHelper = new ContextHelper<TDbContext>(CounterManager, dbContext, CounterKey);
    }
}
