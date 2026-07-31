using EmoteService.Core.Entity;
using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;

namespace EmoteService.Handlers.Suggestions;

public class EmoteSuggestionMessageCreatedHandler(
    IServiceProvider serviceProvider
) : EventHandlerBaseWithDb<EmoteServiceContext>(serviceProvider)
{
    public async Task HandleAsync(EmoteSuggestionMessageCreatedPayload message, CancellationToken cancellationToken)
    {
        if (message.SuggestionId == Guid.Empty)
            return;

        var query = DbContext.EmoteSuggestions.Where(o => o.Id == message.SuggestionId);
        var suggestion = await ContextHelper.ReadFirstOrDefaultEntityAsync(query, cancellationToken);
        if (suggestion is null)
            return;

        suggestion.SuggestionMessageId = message.MessageId;

        await ContextHelper.SaveChangesAsync(cancellationToken);
    }
}
