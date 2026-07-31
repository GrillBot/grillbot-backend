using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;
using Microsoft.EntityFrameworkCore;

namespace EmoteService.Handlers.Suggestions;

public class EmoteSuggestionCancelVoteHandler(IServiceProvider serviceProvider) : EmoteSuggestionHandlerBase(serviceProvider)
{
    public async Task HandleAsync(EmoteSuggestionCancelVotePayload message, CancellationToken cancellationToken)
    {
        if (message.SuggestionId == Guid.Empty)
            return;

        var suggestionQuery = DbContext.EmoteSuggestions
            .Include(o => o.VoteSession)
            .Where(o =>
                o.Id == message.SuggestionId &&
                o.VoteSession != null &&
                o.VoteSession.KilledAtUtc == null &&
                o.VoteSession.ExpectedVoteEndAtUtc >= DateTime.UtcNow &&
                !o.VoteSession.IsClosed
            );

        var suggestion = await ContextHelper.ReadFirstOrDefaultEntityAsync(suggestionQuery, cancellationToken);
        if (suggestion == null)
            return;

        var guildQuery = DbContext.Guilds.Where(o => o.GuildId == suggestion.GuildId);
        var guild = await ContextHelper.ReadFirstOrDefaultEntityAsync(guildQuery, cancellationToken);
        if (guild == null)
            return;

        suggestion.VoteSession!.KilledAtUtc = DateTime.UtcNow;

        await ContextHelper.SaveChangesAsync(cancellationToken);
        var notificationMessage = CreateAdminChannelNotification(suggestion, guild, suggestion.SuggestionMessageId);
        await Publisher.PublishAsync((DiscordEditMessagePayload)notificationMessage);
    }
}
