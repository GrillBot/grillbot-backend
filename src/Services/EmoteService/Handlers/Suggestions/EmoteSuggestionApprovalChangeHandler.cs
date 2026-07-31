using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;

namespace EmoteService.Handlers.Suggestions;

public class EmoteSuggestionApprovalChangeHandler(IServiceProvider serviceProvider) : EmoteSuggestionHandlerBase(serviceProvider)
{
    public async Task HandleAsync(EmoteSuggestionApprovalChangePayload message, CancellationToken cancellationToken)
    {
        if (message.SuggestionId == Guid.Empty)
            return;

        ArgumentOutOfRangeException.ThrowIfZero(message.ApprovedByUserId);

        var suggestionQuery = DbContext.EmoteSuggestions.Where(o => o.Id == message.SuggestionId && o.VoteSession == null);
        var suggestion = await ContextHelper.ReadFirstOrDefaultEntityAsync(suggestionQuery, cancellationToken);

        if (suggestion == null)
            return;

        var guildQuery = DbContext.Guilds.Where(o => o.GuildId == suggestion.GuildId && o.SuggestionChannelId != 0);
        var guild = await ContextHelper.ReadFirstOrDefaultEntityAsync(guildQuery, cancellationToken);

        if (guild is null)
        {
            Logger.LogWarning("Rejecting EmoteSuggestionApprovalChange message, because guild with suggestion channel is not configured.");
            return;
        }

        suggestion.ApprovedForVote = message.IsApprovedForVote;
        suggestion.ApprovalByUserId = message.ApprovedByUserId;
        suggestion.ApprovalSetAtUtc = DateTime.UtcNow;
        await ContextHelper.SaveChangesAsync(cancellationToken);

        var notificationMesasge = CreateAdminChannelNotification(suggestion, guild, suggestion.SuggestionMessageId);
        await Publisher.PublishAsync((DiscordEditMessagePayload)notificationMesasge);
    }
}
