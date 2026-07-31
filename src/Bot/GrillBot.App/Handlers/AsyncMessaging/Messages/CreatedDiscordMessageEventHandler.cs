using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Emote.Events.Suggestions;
using GrillBot.Contracts.Bot.Events.Messages;
using GrillBot.Contracts.Remind.Events;
using Microsoft.Extensions.Logging;
using Wolverine;


namespace GrillBot.App.Handlers.AsyncMessaging.Messages;

public class CreatedDiscordMessageEventHandler(IMessageBus _rabbitPublisher)
{
    public async Task HandleAsync(CreatedDiscordMessagePayload message)
    {
        switch (message.ServiceId)
        {
            case "Remind":
                await ProcessRemindServiceMessageAsync(message);
                break;
            case "Emote":
                await ProcessEmoteServiceMessageAsync(message);
                break;
        }

        return;
    }

    private ValueTask ProcessRemindServiceMessageAsync(CreatedDiscordMessagePayload payload)
    {
        if (payload.ServiceData.TryGetValue("RemindId", out var _remindId) && int.TryParse(_remindId, CultureInfo.InvariantCulture, out var remindId))
            return _rabbitPublisher.PublishAsync(new RemindMessageNotifyPayload(remindId, payload.MessageId));
        return ValueTask.CompletedTask;
    }

    private ValueTask ProcessEmoteServiceMessageAsync(CreatedDiscordMessagePayload payload)
    {
        if (
            !payload.ServiceData.TryGetValue("SuggestionId", out var _suggestionId) ||
            !Guid.TryParse(_suggestionId, out var suggestionId) ||
            !payload.ServiceData.TryGetValue("MessageType", out var messageType)
        )
        {
            return ValueTask.CompletedTask;
        }

        return messageType switch
        {
            "VoteMessage" => _rabbitPublisher.PublishAsync(new EmoteSuggestionVoteMessageCreatedPayload(suggestionId, payload.MessageId.ToUlong())),
            "SuggestionMessage" => _rabbitPublisher.PublishAsync(new EmoteSuggestionMessageCreatedPayload(suggestionId, payload.MessageId.ToUlong())),
            _ => ValueTask.CompletedTask,
        };
    }
}
