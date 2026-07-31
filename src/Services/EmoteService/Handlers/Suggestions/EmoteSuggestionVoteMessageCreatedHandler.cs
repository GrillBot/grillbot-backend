using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace EmoteService.Handlers.Suggestions;

public class EmoteSuggestionVoteMessageCreatedHandler(IServiceProvider serviceProvider) : EmoteSuggestionHandlerBase(serviceProvider)
{
    public async Task HandleAsync(EmoteSuggestionVoteMessageCreatedPayload message, CancellationToken cancellationToken)
    {
        if (message.SuggestionId == Guid.Empty)
            return;

        var suggestionQuery = DbContext.EmoteSuggestions
            .Include(o => o.VoteSession)
            .Where(o => o.Id == message.SuggestionId && o.VoteSession != null);

        var suggestion = await ContextHelper.ReadFirstOrDefaultEntityAsync(suggestionQuery, cancellationToken);
        if (suggestion == null)
            return;

        suggestion.VoteSession!.VoteMessageId = message.MessageId;
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
