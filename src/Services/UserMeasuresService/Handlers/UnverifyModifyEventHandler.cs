using GrillBot.Core.Infrastructure.Auth;
using UserMeasuresService.Handlers.Abstractions;
using GrillBot.Contracts.UserMeasures.Events;

namespace UserMeasuresService.Handlers;

public class UnverifyModifyEventHandler(
    IServiceProvider serviceProvider
) : BaseMeasuresHandler(serviceProvider)
{
    public async Task HandleAsync(UnverifyModifyPayload message, CancellationToken cancellationToken)
    {
        var item = await ContextHelper.ReadFirstOrDefaultEntityAsync(DbContext.Unverifies.Where(o => o.LogSetId == message.LogSetId), cancellationToken);
        if (item is null)
            return;

        if (message.NewEndUtc.HasValue)
            item.ValidTo = message.NewEndUtc.Value.ToUniversalTime();

        await SaveEntityAsync(item);
    }
}
