using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Contracts.UserManagement.Events;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Infractions;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace GrillBot.App.Actions.Api.V2.User;

public class CreateInfractionTimeout : ApiAction
{
    private readonly IMessageBus _publisher;

    public CreateInfractionTimeout(ApiRequestContext apiContext, IMessageBus publisher) : base(apiContext)
    {
        _publisher = publisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = GetParameter<CreateInfractionTimeoutParams>(0);

        var payload = new TimeoutPayload(
            parameters.CreatedAtUtc.ToUniversalTime(),
            parameters.Reason,
            parameters.GuildId.ToUlong(),
            parameters.ModeratorId.ToUlong(),
            parameters.TargetUserId.ToUlong(),
            parameters.ValidToUtc.ToUniversalTime(),
            parameters.TimeoutId
        );

        await _publisher.PublishAsync(payload);
        return new ApiResult(StatusCodes.Status201Created);
    }
}
