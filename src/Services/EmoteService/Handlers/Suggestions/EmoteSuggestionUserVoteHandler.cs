using EmoteService.Core.Entity.Suggestions;
using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;
using Microsoft.EntityFrameworkCore;

namespace EmoteService.Handlers.Suggestions;

public class EmoteSuggestionUserVoteHandler(IServiceProvider serviceProvider) : EmoteSuggestionHandlerBase(serviceProvider)
{
    public async Task HandleAsync(EmoteSuggestionUserVotePayload message, CancellationToken cancellationToken)
    {
        if (message.SuggestionId == Guid.Empty)
            return;

        ArgumentOutOfRangeException.ThrowIfZero(message.UserId);

        var suggestionQuery = DbContext.EmoteSuggestions
            .Include(o => o.VoteSession).ThenInclude(o => o!.UserVotes)
            .Where(o =>
                o.Id == message.SuggestionId &&
                o.VoteSession != null &&
                o.VoteSession.KilledAtUtc == null &&
                !o.VoteSession.IsClosed
            );

        var suggestion = await ContextHelper.ReadFirstOrDefaultEntityAsync(suggestionQuery, cancellationToken);
        if (suggestion == null)
            return;

        var guildQuery = DbContext.Guilds.Where(o => o.GuildId == suggestion.GuildId && o.SuggestionChannelId != 0);
        var guild = await ContextHelper.ReadFirstOrDefaultEntityAsync(guildQuery, cancellationToken);
        if (guild is null)
            return;

        var userVote = suggestion.VoteSession!.UserVotes.FirstOrDefault(o => o.UserId == message.UserId);
        if (userVote is null)
        {
            userVote = new EmoteUserVote
            {
                UserId = message.UserId
            };

            suggestion.VoteSession.UserVotes.Add(userVote);
        }

        userVote.IsApproved = message.IsApproved;
        userVote.UpdatedAtUtc = DateTime.UtcNow;
        await ContextHelper.SaveChangesAsync(cancellationToken);

        var notificationMessage = CreateAdminChannelNotification(suggestion, guild, suggestion.SuggestionMessageId);
        await Publisher.PublishAsync((DiscordEditMessagePayload)notificationMessage);
    }
}
