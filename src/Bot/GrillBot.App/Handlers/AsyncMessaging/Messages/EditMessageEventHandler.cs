using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;
using Microsoft.Extensions.Logging;
using Wolverine;


namespace GrillBot.App.Handlers.AsyncMessaging.Messages;

public class EditMessageEventHandler(
    ILoggerFactory loggerFactory,
    LocalizationManager localizationManager,
    IDiscordClient discordClient,
    IMessageBus rabbitPublisher
) : MessageEventHandlerBase<DiscordEditMessagePayload>(loggerFactory, discordClient, localizationManager, rabbitPublisher)
{
    public async Task HandleAsync(DiscordEditMessagePayload message, CancellationToken cancellationToken)
    {
        await DiscordClient.WaitOnConnectedState(cancellationToken);

        var channel = await GetChannelAsync(message.GuildId, message.ChannelId, cancellationToken);
        var embed = await CreateEmbedAsync(message, cancellationToken);
        var allowedMentions = message.AllowedMentions?.ToAllowedMentions();
        var flags = message.Flags ?? MessageFlags.None;
        var components = (message.Components?.BuildComponents() ?? []).Select(o => o.ToBuilder());
        var wrappedComponents = components is null ? null : ComponentsHelper.CreateWrappedComponents(components.ToList().AsReadOnly());
        var content = await CreateContentAsync(message, cancellationToken);
        var fileAttachments = message.Attachments.ConvertAll(o => o.ToFileAttachment());

        if (channel is null)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, $"Unable to find channel with ID {message.ChannelId}", message, cancellationToken);
            return;
        }

        if (embed is null && string.IsNullOrEmpty(content) && message.Attachments.Count == 0)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, "Unable to edit discord message without content.", message, cancellationToken);
            return;
        }

        try
        {
            await channel.ModifyMessageAsync(message.MessageId, props =>
            {
                props.AllowedMentions = allowedMentions;
                props.Flags = flags;
                props.Embed = embed;
                props.Content = content;
                props.Components = wrappedComponents;
                props.Attachments = fileAttachments;
            }, options: new() { CancelToken = cancellationToken });
        }
        catch (HttpException ex) when (
            ex.DiscordCode is DiscordErrorCode.MaximumNumberOfEditsReached or DiscordErrorCode.CannotEditOtherUsersMessage or DiscordErrorCode.UnknownMessage
        )
        {
            return;
        }
        finally
        {
            foreach (var attachment in fileAttachments)
                attachment.Dispose();
        }
    }
}
