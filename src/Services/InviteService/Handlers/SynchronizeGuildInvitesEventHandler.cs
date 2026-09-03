using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.Redis.Extensions;
using GrillBot.Services.Common.Discord;
using GrillBot.Core.AsyncMessaging.Extensions;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Wolverine;
using InviteService.Core.Entity;
using InviteService.Extensions;
using InviteService.Models.Cache;
using GrillBot.Contracts.Invite.Events;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace InviteService.Handlers;

public class SynchronizeGuildInvitesEventHandler(
    IServiceProvider serviceProvider,
    IServer _redisServer,
    IDatabase _redisDatabase,
    DiscordManager _discordManager,
    IDistributedCache _cache,
    ILogger<SynchronizeGuildInvitesEventHandler> _logger
) : EventHandlerBaseWithDb<InviteContext>(serviceProvider)
{
    public async Task HandleAsync(SynchronizeGuildInvitesPayload message, Envelope envelope, CancellationToken cancellationToken)
    {
        await ClearInvitesForGuildAsync(message.GuildId, cancellationToken);
        await DownloadInvitesToCacheAsync(message.GuildId, envelope.CurrentUser(), message.IgnoreLog, cancellationToken);
    }

    private async Task ClearInvitesForGuildAsync(string guildId, CancellationToken cancellationToken = default)
    {
        var prefix = $"InviteMetadata-{guildId}-*";

        await foreach (var key in _redisServer.KeysAsync(pattern: prefix, pageSize: int.MaxValue))
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Removing invite metadata for key {Key}", key);
            cancellationToken.ThrowIfCancellationRequested();
            await _redisDatabase.KeyDeleteAsync(key);
        }
    }

    private async Task DownloadInvitesToCacheAsync(string guildId, ICurrentUserProvider currentUser, bool ignoreLog, CancellationToken cancellationToken = default)
    {
        var guild = await _discordManager.GetGuildAsync(guildId.ToUlong(), cancellationToken: cancellationToken);

        if (guild is null || !await guild.CanManageInvitesAsync(_discordManager.CurrentUser, cancellationToken))
            return;

        var invites = await _discordManager.GetInvitesAsync(guildId.ToUlong(), cancellationToken);
        if (invites.Count == 0)
            return;

        if (!ignoreLog)
        {
            var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, guildId, currentUser.Id, null, null)
            {
                LogMessage = new LogMessageRequest
                {
                    Message = $"Invites for guild \"{guild.Name}\" was loaded. Loaded invites: {invites.Count}",
                    Source = $"{CounterKey} ({nameof(SynchronizeGuildInvitesEventHandler)})",
                    SourceAppName = "InviteService"
                }
            };

            await Publisher.PublishAsync(new CreateItemsMessage(logRequest));
        }

        foreach (var invite in invites)
        {
            var metadata = InviteMetadata.Create(invite);
            var key = $"InviteMetadata-{invite.GuildId}-{invite.Code}";

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Storing invite metadata for key {Key}", key);
            await _cache.SetAsync(key, metadata, null, cancellationToken);
        }
    }
}
