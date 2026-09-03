using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Contracts.UserMeasures.Events;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace GrillBot.App.Actions.Api.V2.User;

public class CreateUserMeasuresTimeout : ApiAction
{
    private readonly IMessageBus _publisher;

    public CreateUserMeasuresTimeout(ApiRequestContext apiContext, IMessageBus publisher) : base(apiContext)
    {
        _publisher = publisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = GetParameter<CreateUserMeasuresTimeoutParams>(0);

        var payload = new TimeoutPayload(
            parameters.CreatedAtUtc.ToUniversalTime(),
            parameters.Reason,
            parameters.GuildId,
            parameters.ModeratorId,
            parameters.TargetUserId,
            parameters.ValidToUtc.ToUniversalTime(),
            parameters.TimeoutId
        );

        await _publisher.PublishAsync(payload);
        return new ApiResult(StatusCodes.Status201Created);
    }
}
