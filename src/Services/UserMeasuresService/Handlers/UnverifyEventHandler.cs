using GrillBot.Core.Infrastructure.Auth;
using UserMeasuresService.Core.Entity;
using UserMeasuresService.Handlers.Abstractions;
using GrillBot.Contracts.UserMeasures.Events;

namespace UserMeasuresService.Handlers;

public class UnverifyEventHandler(
    IServiceProvider serviceProvider
) : BaseMeasuresHandler(serviceProvider)
{
    public async Task HandleAsync(UnverifyPayload message, CancellationToken cancellationToken)
    {
        var entity = new UnverifyItem
        {
            CreatedAtUtc = message.CreatedAtUtc.ToUniversalTime(),
            GuildId = message.GuildId,
            ModeratorId = message.ModeratorId,
            Reason = message.Reason,
            UserId = message.TargetUserId,
            ValidTo = message.EndAtUtc.ToUniversalTime(),
            LogSetId = message.LogSetId
        };

        await SaveEntityAsync(entity);
    }
}
