using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.UserManagement.Events;

namespace UserManagementService.Handlers.Infractions;

public class UnverifyModifyEventHandler(
    IServiceProvider serviceProvider
) : BaseInfractionHandler(serviceProvider)
{
    public async Task HandleAsync(UnverifyModifyPayload message, CancellationToken cancellationToken)
    {
        var item = await ContextHelper.ReadFirstOrDefaultEntityAsync(
            DbContext.Unverifies.Where(o => o.LogSetId == message.LogSetId),
            cancellationToken
        );

        if (item is null)
            return;

        if (message.NewEndUtc.HasValue)
            item.ValidTo = message.NewEndUtc.Value.ToUniversalTime();

        await SaveEntityAsync(item, cancellationToken);
    }
}
