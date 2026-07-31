using GrillBot.Core.Infrastructure.Auth;
using PointsService.Core.Entity;
using PointsService.Handlers.Abstractions;
using GrillBot.Contracts.Points.Channels;
using GrillBot.Contracts.Points.Events;
using GrillBot.Contracts.Points.Users;

namespace PointsService.Handlers;

public class SynchronizationEventHandler(
    IServiceProvider serviceProvider
) : BasePointsEvent(serviceProvider)
{
    public async Task HandleAsync(SynchronizationPayload message, CancellationToken cancellationToken)
    {
        foreach (var userInfo in message.Users)
            await SynchronizeUserAsync(message.GuildId, userInfo);

        foreach (var channelInfo in message.Channels)
            await SynchronizeChannelAsync(message.GuildId, channelInfo);

        await ContextHelper.SaveChangesAsync(cancellationToken);
        return;
    }

    private async Task SynchronizeUserAsync(string guildId, UserSyncItem user)
    {
        var userQuery = DbContext.Users.Where(o => o.GuildId == guildId && o.Id == user.Id);
        var entity = await ContextHelper.ReadFirstOrDefaultEntityAsync(userQuery);

        if (entity is null)
        {
            entity = new User
            {
                Id = user.Id,
                GuildId = guildId
            };

            await DbContext.AddAsync(entity);
        }

        if (user.PointsDisabled is not null)
            entity.PointsDisabled = user.PointsDisabled.Value;

        if (user.IsUser is not null)
            entity.IsUser = user.IsUser.Value;
    }

    private async Task SynchronizeChannelAsync(string guildId, ChannelSyncItem channel)
    {
        var channelQuery = DbContext.Channels.Where(o => o.GuildId == guildId && o.Id == channel.Id);
        var entity = await ContextHelper.ReadFirstOrDefaultEntityAsync(channelQuery);

        if (entity is null)
        {
            entity = new Channel
            {
                Id = channel.Id,
                GuildId = guildId
            };

            await DbContext.AddAsync(entity);
        }

        if (channel.IsDeleted is not null)
            entity.IsDeleted = channel.IsDeleted.Value;

        if (channel.PointsDisabled is not null)
            entity.PointsDisabled = channel.PointsDisabled.Value;
    }
}
