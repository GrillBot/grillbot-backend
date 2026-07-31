using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.Bot.Events.Messages;
using GrillBot.Data.Models;
using Microsoft.Extensions.Logging;
using Wolverine;


namespace GrillBot.App.Handlers.AsyncMessaging.Messages;

public class SendMessageEventHandler(
    ILoggerFactory loggerFactory,
    IDiscordClient discordClient,
    IMessageBus rabbitPublisher,
    LocalizationManager localizationManager
) : MessageEventHandlerBase<DiscordSendMessagePayload>(loggerFactory, discordClient, localizationManager, rabbitPublisher)
{
    public async Task HandleAsync(DiscordSendMessagePayload message, CancellationToken cancellationToken)
    {
        await DiscordClient.WaitOnConnectedState(cancellationToken);

        var channel = await GetChannelAsync(message.GuildId, message.ChannelId, cancellationToken);
        var embed = await CreateEmbedAsync(message, cancellationToken);
        var allowedMentions = message.AllowedMentions?.ToAllowedMentions();
        var flags = message.Flags ?? MessageFlags.None;
        var components = (message.Components?.BuildComponents() ?? []).Select(o => o.ToBuilder());
        var wrappedComponents = components is null ? null : ComponentsHelper.CreateWrappedComponents(components.ToList().AsReadOnly());
        var content = await CreateContentAsync(message, cancellationToken);
        var reference = message.Reference?.ToDiscordReference();

        if (channel is null)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, $"Unable to find channel with ID {message.ChannelId}", message, cancellationToken);
            return;
        }

        if (embed is null && string.IsNullOrEmpty(content) && message.Attachments.Count == 0)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, "Unable to send discord message without content.", message, cancellationToken);
            return;
        }

        IUserMessage? msg = null;
        try
        {
            if (message.Attachments.Count > 0)
            {
                msg = await SendMessageWithAttachmentsAsync(channel, content, allowedMentions, message.Attachments, embed, flags, wrappedComponents, reference, cancellationToken);
                return;
            }

            msg = await channel.SendMessageAsync(
                text: content,
                isTTS: false,
                embed: embed,
                options: null,
                allowedMentions: allowedMentions,
                messageReference: reference,
                components: wrappedComponents,
                stickers: null,
                embeds: null,
                flags: flags
            );
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            msg = new EmptyUserMessage(0);
        }
        finally
        {
            if (msg is not null)
                await PublishCreatedMessageAsync(message, msg, cancellationToken);
        }

        return;
    }

    private static async Task<IUserMessage> SendMessageWithAttachmentsAsync(IMessageChannel channel, string? content, AllowedMentions? allowedMentions, List<DiscordMessageFile> attachments,
        Embed? embed, MessageFlags flags, MessageComponent? components, MessageReference? reference, CancellationToken cancellationToken = default)
    {
        var fileAttachments = attachments.ConvertAll(o => o.ToFileAttachment());

        try
        {
            return await channel.SendFilesAsync(
                attachments: fileAttachments,
                text: content,
                isTTS: false,
                embed: embed,
                options: new() { CancelToken = cancellationToken },
                allowedMentions: allowedMentions,
                messageReference: reference,
                components: components,
                stickers: null,
                embeds: null,
                flags: flags
            );
        }
        finally
        {
            foreach (var attachment in fileAttachments)
                attachment.Dispose();
        }
    }

    private Task PublishCreatedMessageAsync(DiscordSendMessagePayload payload, IUserMessage message, CancellationToken cancellationToken = default)
    {
        var resultMessage = new CreatedDiscordMessagePayload(
            payload.GuildId?.ToString(),
            payload.ChannelId.ToString(),
            message.Id.ToString(),
            payload.ServiceId,
            payload.ServiceData
        );

        return RabbitPublisher.PublishAsync(resultMessage).AsTask();
    }
}
