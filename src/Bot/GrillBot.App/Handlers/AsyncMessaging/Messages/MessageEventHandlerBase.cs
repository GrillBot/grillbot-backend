using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Contracts.Bot.Events.Messages;
using Microsoft.Extensions.Logging;
using Wolverine;


namespace GrillBot.App.Handlers.AsyncMessaging.Messages;

/// <summary>
/// What the send and edit handlers have in common: resolving the target channel and turning a
/// payload into a localized Discord message. Wolverine discovers the concrete subclasses by
/// their "Handler" suffix, so this base intentionally declares no message type of its own.
/// </summary>
public abstract class MessageEventHandlerBase<TPayload>(
    ILoggerFactory _loggerFactory,
    IDiscordClient _discordClient,
    LocalizationManager _localizationManager,
    IMessageBus _rabbitPublisher
) where TPayload : DiscordMessagePayloadData
{
    protected IDiscordClient DiscordClient => _discordClient;
    protected IMessageBus RabbitPublisher => _rabbitPublisher;
    protected ILogger Logger => _loggerFactory.CreateLogger(GetType());

    protected async Task<IMessageChannel?> GetChannelAsync(ulong? guildId, ulong channelId, CancellationToken cancellationToken = default)
    {
        if (guildId is not null)
        {
            var guild = await _discordClient.GetGuildAsync(guildId.Value, options: new() { CancelToken = cancellationToken });
            return guild is null ? null : await guild.GetTextChannelAsync(channelId, options: new() { CancelToken = cancellationToken });
        }

        var user = await _discordClient.GetUserAsync(channelId, options: new() { CancelToken = cancellationToken });
        return user is null ? null : (IMessageChannel)await user.CreateDMChannelAsync(options: new() { CancelToken = cancellationToken });
    }

    protected Task LogWarningAsync(ulong? guildId, ulong channelId, string message, TPayload payload, CancellationToken cancellationToken = default)
    {
        var logRequest = new LogRequest(LogType.Warning, DateTime.UtcNow, guildId?.ToString(), _discordClient.CurrentUser.Id.ToString(), channelId.ToString())
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"{message}\n{System.Text.Json.JsonSerializer.Serialize(payload)}",
                SourceAppName = "GrillBot",
                Source = $"AsyncMessaging/{typeof(TPayload).Name}/{GetType().Name}"
            }
        };

        Logger.LogWarning("{Message}", message);
        return RabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest)).AsTask();
    }

    protected async Task<Embed?> CreateEmbedAsync(TPayload payload, CancellationToken cancellationToken = default)
    {
        if (payload.Embed is null)
            return null;

        if (!payload.CanUseLocalization)
            return !payload.Embed.IsValidEmbed() ? null : payload.Embed.ToBuilder().Build();

        if (!payload.ServiceData.TryGetValue("Language", out var language))
            language = TextsManager.DefaultLocale;

        var localizedEmbed = await _localizationManager.CreateLocalizedEmbedAsync(payload.Embed, language, payload.ServiceData, cancellationToken);
        return localizedEmbed.Build();
    }

    protected async Task<string?> CreateContentAsync(TPayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(payload.Content?.Key))
            return null;
        if (!payload.CanUseLocalization)
            return payload.Content.Key;

        var locale = payload.Locale ?? TextsManager.DefaultLocale;
        return await _localizationManager.TransformValueAsync(payload.Content!, locale, payload.ServiceData, cancellationToken);
    }
}
