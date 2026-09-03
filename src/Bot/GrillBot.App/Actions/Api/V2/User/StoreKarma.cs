using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Contracts.Rubbergod.Events.Karma;
using GrillBot.Data.Models.API.Users;
using GrillBot.Core.AsyncMessaging.Extensions;
using Wolverine;


namespace GrillBot.App.Actions.Api.V2.User;

public class StoreKarma : ApiAction
{
    private readonly IMessageBus _rabbitPublisher;

    public StoreKarma(ApiRequestContext apiContext, IMessageBus rabbitPublisher) : base(apiContext)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var items = GetParameter<List<RawKarmaItem>>(0);

        var batches = items
            .Select(o => new KarmaUser(o.MemberId, o.KarmaValue, o.Positive, o.Negative))
            .Chunk(100)
            .Select(ch => new KarmaBatchPayload([.. ch]))
            .ToList();

        await _rabbitPublisher.PublishAllAsync(batches);
        return ApiResult.Ok();
    }
}
