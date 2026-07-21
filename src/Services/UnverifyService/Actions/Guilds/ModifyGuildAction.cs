using GrillBot.Core.Extensions;
using UnverifyService.Models.Events;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Services.Common.Infrastructure.Api;
using UnverifyService.Core.Entity;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Contracts.Unverify.Responses.Guilds;

namespace UnverifyService.Actions.Guilds;

public class ModifyGuildAction(
    IServiceProvider serviceProvider,
    IRabbitPublisher _rabbitPublisher
) : ApiAction<GetGuildInfoAction, UnverifyContext>(serviceProvider)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = GetParameter<ulong>(0);
        var request = GetParameter<ModifyGuildRequest>(1);

        var guildQuery = DbContext.Guilds.Where(o => o.GuildId == guildId);
        var guildEntity = await ContextHelper.ReadFirstOrDefaultEntityAsync(guildQuery, CancellationToken);

        if (guildEntity is null)
        {
            guildEntity = new Guild
            {
                GuildId = guildId
            };

            await DbContext.AddAsync(guildEntity, CancellationToken);
        }

        guildEntity.MuteRoleId = request.MuteRoleId.ToUlong();
        await ContextHelper.SaveChangesAsync(CancellationToken);
        await _rabbitPublisher.PublishAsync(new RecalculateMetricsMessage(), cancellationToken: CancellationToken);

        return await ExecuteParentAction([guildId]);
    }
}
