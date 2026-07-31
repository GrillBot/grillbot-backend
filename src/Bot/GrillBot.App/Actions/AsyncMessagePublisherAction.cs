using GrillBot.Common.Models;
using GrillBot.Core.AsyncMessaging.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using Wolverine;

namespace GrillBot.App.Actions;

/// <summary>
/// Publishes an integration event on behalf of the caller. The exchange and routing key come
/// from the message topology, so the payload is the only parameter this action needs.
/// </summary>
public class AsyncMessagePublisherAction(ApiRequestContext apiContext, IMessageBus _publisher) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        // CurrentUser is supplied by Init, so the caller's token travels with the message.
        await _publisher.PublishAsync(Parameters[0]!, CurrentUser);
        return ApiResult.Ok();
    }
}
