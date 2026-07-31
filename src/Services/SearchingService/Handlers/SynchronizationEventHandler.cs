using Discord;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using SearchingService.Core.Entity;
using GrillBot.Contracts.Searching.Events;
using GrillBot.Contracts.Searching.Events.Users;

namespace SearchingService.Handlers;

public class SynchronizationEventHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<SearchingServiceContext>(serviceProvider)
{
    public async Task HandleAsync(SynchronizationPayload message, CancellationToken cancellationToken)
    {
        foreach (var user in message.Users)
            await SynchonizeUserAsync(user);

        await ContextHelper.SaveChangesAsync(cancellationToken);
        return;
    }

    private async Task SynchonizeUserAsync(UserSynchronizationItem user)
    {
        var entity = await ContextHelper.ReadFirstOrDefaultEntityAsync<User>(o => o.GuildId == user.GuildId && o.UserId == user.UserId);

        if (entity is null)
        {
            entity = new User
            {
                UserId = user.UserId,
                GuildId = user.GuildId
            };

            await DbContext.AddAsync(entity);
        }

        var permissions = new GuildPermissions((ulong)user.GuildPermissions);
        entity.HaveGuildAdmin = permissions.Administrator;
        entity.HaveManageMessages = permissions.ManageMessages;
        entity.IsSearchingAdmin = user.IsAdmin;
    }
}
