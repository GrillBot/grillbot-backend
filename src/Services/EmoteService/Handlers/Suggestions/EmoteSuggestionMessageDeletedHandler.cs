using EmoteService.Core.Entity;
using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.EntityFrameworkCore;

namespace EmoteService.Handlers.Suggestions;

public class EmoteSuggestionMessageDeletedHandler(IServiceProvider serviceProvider) : EventHandlerBaseWithDb<EmoteServiceContext>(serviceProvider)
{
    public async Task HandleAsync(EmoteSuggestionMessageDeletedPayload message, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfZero(message.GuildId);
        ArgumentOutOfRangeException.ThrowIfZero(message.MessageId);

        var query = DbContext.EmoteSuggestions
            .Include(o => o.VoteSession)
            .Where(o => o.GuildId == message.GuildId && o.SuggestionMessageId == message.MessageId);

        var suggestion = await ContextHelper.ReadFirstOrDefaultEntityAsync(query, cancellationToken);
        if (suggestion == null)
            return;

        suggestion.ApprovedForVote = false;
        suggestion.ApprovalSetAtUtc = null;
        suggestion.ApprovalByUserId = null;

        if (suggestion.VoteSession is not null)
            suggestion.VoteSession.KilledAtUtc = DateTime.UtcNow;

        await ContextHelper.SaveChangesAsync(cancellationToken);
    }
}
