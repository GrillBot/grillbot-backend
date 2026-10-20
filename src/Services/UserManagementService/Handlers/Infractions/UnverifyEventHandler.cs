using GrillBot.Core.Infrastructure.Auth;
using UserManagementService.Core.Entity.Infractions;
using GrillBot.Contracts.UserManagement.Events;

namespace UserManagementService.Handlers.Infractions;

public class UnverifyEventHandler(
    IServiceProvider serviceProvider
) : BaseInfractionHandler(serviceProvider)
{
    public async Task HandleAsync(UnverifyPayload message, CancellationToken cancellationToken)
    {
        var entity = new UnverifyInfraction
        {
            CreatedAtUtc = message.CreatedAtUtc.ToUniversalTime(),
            GuildId = message.GuildId,
            ModeratorId = message.ModeratorId,
            Reason = message.Reason,
            UserId = message.TargetUserId,
            ValidTo = message.EndAtUtc.ToUniversalTime(),
            LogSetId = message.LogSetId
        };

        await SaveEntityAsync(entity, cancellationToken);
    }
}
