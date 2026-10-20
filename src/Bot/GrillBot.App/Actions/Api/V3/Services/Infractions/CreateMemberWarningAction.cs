using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Contracts.UserManagement.Events;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Infractions;
using Wolverine;

namespace GrillBot.App.Actions.Api.V3.Services.Infractions;

public class CreateMemberWarningAction(
    ApiRequestContext apiContext,
    IMessageBus _rabbitPublisher
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var request = GetParameter<CreateMemberWarningParams>(0);
        var moderatorId = ApiContext.GetUserId();
        var createdAt = DateTime.UtcNow;

        var payload = new MemberWarningPayload(
            CreatedAtUtc: createdAt,
            Reason: request.Message,
            GuildId: request.GuildId.ToUlong(),
            ModeratorId: moderatorId,
            TargetUserId: request.UserId.ToUlong(),
            SendDmNotification: request.SendDmNotification
        );

        await _rabbitPublisher.PublishAsync(payload);
        return ApiResult.Ok();
    }
}
