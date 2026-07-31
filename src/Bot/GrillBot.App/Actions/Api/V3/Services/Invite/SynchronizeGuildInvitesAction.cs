using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Invite.Events;
using GrillBot.Core.AsyncMessaging.Extensions;
using Wolverine;


namespace GrillBot.App.Actions.Api.V3.Services.Invite;

public class SynchronizeGuildInvitesAction(
    ApiRequestContext context,
    IMessageBus _rabbitPublisher,
    ICurrentUserProvider _currentUser
) : ApiAction(context)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = GetParameter<ulong>(0);
        var payload = new SynchronizeGuildInvitesPayload(guildId.ToString(), false);

        await _rabbitPublisher.PublishAsync(payload, _currentUser);
        await Task.Delay(1000);

        return ApiResult.Ok();
    }
}
