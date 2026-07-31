using EmoteService.Core.Entity;
using EmoteService.Extensions.QueryExtensions;
using GrillBot.Contracts.Emote.Events;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;

namespace EmoteService.Handlers;

public class EmoteEventEventHandler(IServiceProvider serviceProvider) : EventHandlerBaseWithDb<EmoteServiceContext>(serviceProvider)
{
    public async Task HandleAsync(EmoteEventPayload message, CancellationToken cancellationToken)
    {
        var emoteValue = Discord.Emote.Parse(message.EmoteId);
        if (message.IsIncrement && !await IsSupportedEmoteAsync(emoteValue))
            return;

        var entity = await GetEntityAsync(message.GuildId, message.UserId, emoteValue);
        if (entity is null)
        {
            if (message.IsIncrement)
                entity = await CreateEntityAsync(message.GuildId, message.UserId, emoteValue);
            else
                return;
        }

        if (message.IsIncrement)
        {
            if (entity.FirstOccurence == DateTime.MinValue)
                entity.FirstOccurence = message.EventCreatedAt;

            entity.LastOccurence = message.EventCreatedAt;
            entity.UseCount++;
        }
        else
        {
            entity.UseCount--;

            if (entity.UseCount <= 0)
                DbContext.Remove(entity);
        }

        await ContextHelper.SaveChangesAsync(cancellationToken);
        return;
    }

    private void ValidationFailed(string message)
    {
        if (Logger.IsEnabled(LogLevel.Warning))
            Logger.LogWarning(new EventId(2, "ValidationFailed_PublishAudit"), "{Message}", message);
    }

    private async Task<bool> IsSupportedEmoteAsync(Discord.Emote emote)
    {
        var query = DbContext.EmoteDefinitions.WithEmoteQuery(emote);
        var isSupported = await ContextHelper.IsAnyAsync(query);

        if (!isSupported)
            ValidationFailed($"Unsupported emote {emote}");
        return isSupported;
    }

    private async Task<EmoteUserStatItem?> GetEntityAsync(string guildId, string userId, Discord.Emote emote)
    {
        var query = DbContext.EmoteUserStatItems.Where(o => o.GuildId == guildId && o.UserId == userId).WithEmoteQuery(emote);
        return await ContextHelper.ReadFirstOrDefaultEntityAsync(query);
    }

    private async Task<EmoteUserStatItem> CreateEntityAsync(string guildId, string userId, Discord.Emote emote)
    {
        var entity = new EmoteUserStatItem
        {
            GuildId = guildId,
            EmoteId = emote.Id.ToString(),
            EmoteIsAnimated = emote.Animated,
            UserId = userId,
            EmoteName = emote.Name
        };

        await DbContext.AddAsync(entity);
        return entity;
    }
}
