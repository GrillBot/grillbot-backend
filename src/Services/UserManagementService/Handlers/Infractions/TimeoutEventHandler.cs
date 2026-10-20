using UserManagementService.Core.Entity.Infractions;
using GrillBot.Contracts.UserManagement.Events;

namespace UserManagementService.Handlers.Infractions;

public class TimeoutEventHandler(
    IServiceProvider serviceProvider
) : BaseInfractionHandler(serviceProvider)
{
    public async Task HandleAsync(TimeoutPayload message, CancellationToken cancellationToken)
    {
        var entity = await GetOrCreateEntityAsync(message.ExternalId, cancellationToken);

        entity.CreatedAtUtc = message.CreatedAtUtc;
        entity.GuildId = message.GuildId;
        entity.ModeratorId = message.ModeratorId;
        entity.Reason = message.Reason;
        entity.UserId = message.TargetUserId;
        entity.ValidTo = message.ValidToUtc;

        await SaveEntityAsync(entity, cancellationToken);
    }

    private async Task<TimeoutInfraction> GetOrCreateEntityAsync(long externalId, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Timeouts.Where(o => o.ExternalId == externalId);
        var item = await ContextHelper.ReadFirstOrDefaultEntityAsync(query, cancellationToken);

        return item ?? new TimeoutInfraction { ExternalId = externalId };
    }
}
