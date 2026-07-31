using EmoteService.Core.Entity;
using GrillBot.Contracts.Emote.Events.Guild;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;

namespace EmoteService.Handlers.Guild;

public class GuildChannelDeletedHandler(IServiceProvider serviceProvider) : EventHandlerBaseWithDb<EmoteServiceContext>(serviceProvider)
{
    public async Task HandleAsync(GuildChannelDeletedPayload message, CancellationToken cancellationToken)
    {
        if (message.GuildId == 0 || message.ChannelId == 0)
            return;

        var guildQuery = DbContext.Guilds
            .Where(o => o.GuildId == message.GuildId);

        var guild = await ContextHelper.ReadFirstOrDefaultEntityAsync(guildQuery, cancellationToken);
        if (guild is null)
            return;

        // Clear configuration if channel is private channel for suggestions.
        if (guild.SuggestionChannelId == message.ChannelId)
            guild.SuggestionChannelId = 0;

        // Clear configuration if channel is channel for votes and kill active vote sessions.
        if (guild.VoteChannelId == message.ChannelId)
        {
            guild.VoteChannelId = 0;

            // Kill all active vote sessions for this guild.
            var voteSessionsQuery = DbContext.EmoteVoteSessions
                .Where(o => o.KilledAtUtc == null && o.ExpectedVoteEndAtUtc > DateTime.UtcNow && o.Suggestion.GuildId == message.GuildId);

            var votes = await ContextHelper.ReadEntitiesAsync(voteSessionsQuery, cancellationToken);
            foreach (var vote in votes)
                vote.KilledAtUtc = DateTime.UtcNow;
        }

        await ContextHelper.SaveChangesAsync(cancellationToken);
    }
}
