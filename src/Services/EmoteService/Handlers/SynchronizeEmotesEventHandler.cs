using EmoteService.Core.Entity;
using GrillBot.Contracts.Emote.Events;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;

namespace EmoteService.Handlers;

public class SynchronizeEmotesEventHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<EmoteServiceContext>(serviceProvider)
{
    public async Task HandleAsync(SynchronizeEmotesPayload message, CancellationToken cancellationToken)
    {
        await ClearEmotesAsync(message.GuildId);
        await InsertEmotesAsync(message.GuildId, message.Emotes);

        return;
    }

    private async Task ClearEmotesAsync(string guildId)
    {
        var query = DbContext.EmoteDefinitions.Where(o => o.GuildId == guildId);
        var emotes = await ContextHelper.ReadEntitiesAsync(query);
        if (emotes.Count == 0)
            return;

        DbContext.RemoveRange(emotes);
        await ContextHelper.SaveChangesAsync();
    }

    private async Task InsertEmotesAsync(string guildId, List<string> emotes)
    {
        if (emotes.Count == 0)
            return;

        var emoteEntities = emotes
            .Select(Discord.Emote.Parse)
            .Select(e => new EmoteDefinition
            {
                EmoteId = e.Id.ToString(),
                EmoteIsAnimated = e.Animated,
                EmoteName = e.Name,
                GuildId = guildId
            });

        await DbContext.AddRangeAsync(emoteEntities);
        await ContextHelper.SaveChangesAsync();
    }
}
