using GrillBot.Core.Infrastructure.Auth;
using UserMeasuresService.Core.Entity;
using UserMeasuresService.Handlers.Abstractions;
using GrillBot.Contracts.UserMeasures.Events;

namespace UserMeasuresService.Handlers;

public class TimeoutEventHandler(
    IServiceProvider serviceProvider
) : BaseMeasuresHandler(serviceProvider)
{
    public async Task HandleAsync(TimeoutPayload message, CancellationToken cancellationToken)
    {
        var entity = await GetOrCreateEntityAsync(message.ExternalId);

        entity.CreatedAtUtc = message.CreatedAtUtc;
        entity.GuildId = message.GuildId;
        entity.ModeratorId = message.ModeratorId;
        entity.Reason = message.Reason;
        entity.UserId = message.TargetUserId;
        entity.ValidTo = message.ValidToUtc;

        await SaveEntityAsync(entity);
        return;
    }

    private async Task<TimeoutItem> GetOrCreateEntityAsync(long externalId)
    {
        var query = DbContext.Timeouts.Where(o => o.ExternalId == externalId);
        var item = await ContextHelper.ReadFirstOrDefaultEntityAsync(query);

        return item ?? new TimeoutItem { ExternalId = externalId };
    }
}
