using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using RemindService.Core.Entity;
using GrillBot.Contracts.Remind.Events;

namespace RemindService.Handlers;

public class RemindMessageNotifyEventHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<RemindServiceContext>(serviceProvider)
{
    public async Task HandleAsync(RemindMessageNotifyPayload message, CancellationToken cancellationToken)
    {
        var messageQuery = DbContext.RemindMessages.Where(o => o.Id == message.RemindId);
        var remindMessage = await ContextHelper.ReadFirstOrDefaultEntityAsync(messageQuery, cancellationToken);
        if (remindMessage is null)
            return;

        remindMessage.IsSendInProgress = false;
        remindMessage.NotificationMessageId = message.NotificationMessageId;

        await ContextHelper.SaveChangesAsync(cancellationToken);
    }
}
