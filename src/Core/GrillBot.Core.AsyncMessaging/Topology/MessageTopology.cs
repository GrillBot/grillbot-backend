using GrillBot.Contracts.Bot.Events.Messages;
using GrillBot.Contracts.Emote.Events.Guild;
using GrillBot.Contracts.Emote.Events.Suggestions;
using Wolverine;
using Wolverine.RabbitMQ;
using AuditLogEvents = GrillBot.Contracts.AuditLog.Events;
using AuditLogCreate = GrillBot.Contracts.AuditLog.Events.Create;
using AuditLogRecalculation = GrillBot.Contracts.AuditLog.Events.Recalculation;
using BotErrors = GrillBot.Contracts.Bot.Events.Errors;
using BotMessaging = GrillBot.Contracts.Bot.Events.MessagingConstants;
using EmoteEvents = GrillBot.Contracts.Emote.Events;
using InviteEvents = GrillBot.Contracts.Invite.Events;
using KarmaEvents = GrillBot.Contracts.Rubbergod.Events.Karma;
using MessageEvents = GrillBot.Contracts.Message.Events;
using PinEvents = GrillBot.Contracts.Rubbergod.Events.Pins;
using PointsEvents = GrillBot.Contracts.Points.Events;
using RemindEvents = GrillBot.Contracts.Remind.Events;
using RubbergodMessaging = GrillBot.Contracts.Rubbergod.Events.MessagingConstants;
using SearchingEvents = GrillBot.Contracts.Searching.Events;
using UnverifyEvents = GrillBot.Contracts.Unverify.Events;
using UserManagementEvents = GrillBot.Contracts.UserManagement.Events;
using UserMeasuresEvents = GrillBot.Contracts.UserMeasures.Events;

namespace GrillBot.Core.AsyncMessaging.Topology;

/// <summary>
/// The complete broker topology in one table: which exchange and routing key every
/// integration event is published with, and which service's queue consumes it.
///
/// Publishing is applied everywhere - any application may send any message. Bindings
/// are applied only for the queue the current application listens to, so a service
/// subscribes to exactly the keys it has a handler for.
/// </summary>
public static class MessageTopology
{
    public static readonly IReadOnlyList<MessageRoute> Routes =
    [
        // Audit log
        new(typeof(AuditLogEvents.BulkDeletePayload), AuditLogEvents.MessagingConstants.Exchange,
            AuditLogEvents.MessagingConstants.RoutingKeys.BulkDelete, Queues.AuditLogService),
        new(typeof(AuditLogCreate.CreateItemsMessage), AuditLogEvents.MessagingConstants.Exchange,
            AuditLogEvents.MessagingConstants.RoutingKeys.CreateItems, Queues.AuditLogService),
        new(typeof(AuditLogRecalculation.RecalculationPayload), AuditLogEvents.MessagingConstants.Exchange,
            AuditLogEvents.MessagingConstants.RoutingKeys.UserRecalculation, Queues.AuditLogService),
        // Owned by the audit log context, but the blob storage it deletes from lives in the bot.
        new(typeof(AuditLogEvents.FileDeletePayload), AuditLogEvents.MessagingConstants.Exchange,
            AuditLogEvents.MessagingConstants.RoutingKeys.FileDelete, Queues.Bot),

        // Bot
        new(typeof(BotErrors.ErrorNotificationPayload), BotMessaging.Exchange,
            BotMessaging.RoutingKeys.ErrorNotification, Queues.Bot),
        new(typeof(CreatedDiscordMessagePayload), BotMessaging.Exchange,
            BotMessaging.RoutingKeys.CreatedMessage, Queues.Bot),
        new(typeof(DiscordEditMessagePayload), BotMessaging.Exchange,
            BotMessaging.RoutingKeys.EditMessage, Queues.Bot),
        new(typeof(DiscordSendMessagePayload), BotMessaging.Exchange,
            BotMessaging.RoutingKeys.SendMessage, Queues.Bot),

        // Emote
        new(typeof(EmoteEvents.EmoteEventPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.EmoteEvent, Queues.EmoteService),
        new(typeof(EmoteEvents.SynchronizeEmotesPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.Synchronization, Queues.EmoteService),
        new(typeof(GuildChannelDeletedPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.GuildChannelDeleted, Queues.EmoteService),
        new(typeof(EmoteSuggestionApprovalChangePayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionApprovalChange, Queues.EmoteService),
        new(typeof(EmoteSuggestionCancelVotePayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionCancelVote, Queues.EmoteService),
        new(typeof(EmoteSuggestionMessageCreatedPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionMessageCreated, Queues.EmoteService),
        new(typeof(EmoteSuggestionMessageDeletedPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionMessageDeleted, Queues.EmoteService),
        new(typeof(EmoteSuggestionRequestPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionRequest, Queues.EmoteService),
        new(typeof(EmoteSuggestionUserVotePayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionUserVote, Queues.EmoteService),
        new(typeof(EmoteSuggestionVoteMessageCreatedPayload), EmoteEvents.MessagingConstants.Exchange,
            EmoteEvents.MessagingConstants.RoutingKeys.SuggestionVoteMessageCreated, Queues.EmoteService),

        // Invite
        new(typeof(InviteEvents.InviteCreatedPayload), InviteEvents.MessagingConstants.Exchange,
            InviteEvents.MessagingConstants.RoutingKeys.InviteCreated, Queues.InviteService),
        new(typeof(InviteEvents.SynchronizeGuildInvitesPayload), InviteEvents.MessagingConstants.Exchange,
            InviteEvents.MessagingConstants.RoutingKeys.SynchronizeGuildInvites, Queues.InviteService),
        new(typeof(InviteEvents.UserJoinedPayload), InviteEvents.MessagingConstants.Exchange,
            InviteEvents.MessagingConstants.RoutingKeys.UserJoined, Queues.InviteService),

        // Message
        new(typeof(MessageEvents.MessageReceivedPayload), MessageEvents.MessagingConstants.Exchange,
            MessageEvents.MessagingConstants.RoutingKeys.MessageReceived, Queues.MessageService),
        new(typeof(MessageEvents.SynchronizationPayload), MessageEvents.MessagingConstants.Exchange,
            MessageEvents.MessagingConstants.RoutingKeys.Synchronization, Queues.MessageService),

        // Points
        new(typeof(PointsEvents.CreateTransactionPayload), PointsEvents.MessagingConstants.Exchange,
            PointsEvents.MessagingConstants.RoutingKeys.CreateTransaction, Queues.PointsService),
        new(typeof(PointsEvents.CreateTransactionAdminPayload), PointsEvents.MessagingConstants.Exchange,
            PointsEvents.MessagingConstants.RoutingKeys.CreateTransactionAdmin, Queues.PointsService),
        new(typeof(PointsEvents.DeleteTransactionsPayload), PointsEvents.MessagingConstants.Exchange,
            PointsEvents.MessagingConstants.RoutingKeys.DeleteTransactions, Queues.PointsService),
        new(typeof(PointsEvents.SynchronizationPayload), PointsEvents.MessagingConstants.Exchange,
            PointsEvents.MessagingConstants.RoutingKeys.Synchronization, Queues.PointsService),
        new(typeof(PointsEvents.UserRecalculationPayload), PointsEvents.MessagingConstants.Exchange,
            PointsEvents.MessagingConstants.RoutingKeys.UserRecalculation, Queues.PointsService),

        // Remind
        new(typeof(RemindEvents.RemindMessageNotifyPayload), RemindEvents.MessagingConstants.Exchange,
            RemindEvents.MessagingConstants.RoutingKeys.RemindMessageNotify, Queues.RemindService),
        new(typeof(RemindEvents.SendRemindNotificationPayload), RemindEvents.MessagingConstants.Exchange,
            RemindEvents.MessagingConstants.RoutingKeys.SendRemindNotification, Queues.RemindService),

        // Rubbergod
        new(typeof(KarmaEvents.KarmaBatchPayload), RubbergodMessaging.Exchange,
            RubbergodMessaging.RoutingKeys.KarmaBatch, Queues.RubbergodService),
        new(typeof(PinEvents.ClearPinCachePayload), RubbergodMessaging.Exchange,
            RubbergodMessaging.RoutingKeys.ClearPinCache, Queues.RubbergodService),

        // Searching
        new(typeof(SearchingEvents.SearchItemPayload), SearchingEvents.MessagingConstants.Exchange,
            SearchingEvents.MessagingConstants.RoutingKeys.CreateSearchItem, Queues.SearchingService),
        new(typeof(SearchingEvents.SynchronizationPayload), SearchingEvents.MessagingConstants.Exchange,
            SearchingEvents.MessagingConstants.RoutingKeys.Synchronization, Queues.SearchingService),

        // Unverify
        new(typeof(UnverifyEvents.GuildUserLeftMessage), UnverifyEvents.MessagingConstants.Exchange,
            UnverifyEvents.MessagingConstants.RoutingKeys.GuildUserLeft, Queues.UnverifyService),
        new(typeof(UnverifyEvents.LogBulkDeleteMessage), UnverifyEvents.MessagingConstants.Exchange,
            UnverifyEvents.MessagingConstants.RoutingKeys.LogBulkDelete, Queues.UnverifyService),
        new(typeof(UnverifyEvents.RecalculateMetricsMessage), UnverifyEvents.MessagingConstants.Exchange,
            UnverifyEvents.MessagingConstants.RoutingKeys.RecalculateMetrics, Queues.UnverifyService),
        new(typeof(UnverifyEvents.RecoverAccessMessage), UnverifyEvents.MessagingConstants.Exchange,
            UnverifyEvents.MessagingConstants.RoutingKeys.RecoverAccess, Queues.UnverifyService),
        new(typeof(UnverifyEvents.SetUnverifyMessage), UnverifyEvents.MessagingConstants.Exchange,
            UnverifyEvents.MessagingConstants.RoutingKeys.SetUnverify, Queues.UnverifyService),
        new(typeof(UnverifyEvents.SynchronizationMessage), UnverifyEvents.MessagingConstants.Exchange,
            UnverifyEvents.MessagingConstants.RoutingKeys.Synchronization, Queues.UnverifyService),

        // User management
        new(typeof(UserManagementEvents.NicknameChangedMessage), UserManagementEvents.MessagingConstants.Exchange,
            UserManagementEvents.MessagingConstants.RoutingKeys.NicknameChanged, Queues.UserManagementService),

        // User measures
        new(typeof(UserMeasuresEvents.MemberWarningPayload), UserMeasuresEvents.MessagingConstants.Exchange,
            UserMeasuresEvents.MessagingConstants.RoutingKeys.MemberWarning, Queues.UserMeasuresService),
        new(typeof(UserMeasuresEvents.TimeoutPayload), UserMeasuresEvents.MessagingConstants.Exchange,
            UserMeasuresEvents.MessagingConstants.RoutingKeys.Timeout, Queues.UserMeasuresService),
        new(typeof(UserMeasuresEvents.UnverifyPayload), UserMeasuresEvents.MessagingConstants.Exchange,
            UserMeasuresEvents.MessagingConstants.RoutingKeys.Unverify, Queues.UserMeasuresService),
        new(typeof(UserMeasuresEvents.UnverifyModifyPayload), UserMeasuresEvents.MessagingConstants.Exchange,
            UserMeasuresEvents.MessagingConstants.RoutingKeys.UnverifyModify, Queues.UserMeasuresService)
    ];

    /// <summary>
    /// Routes every known message type to its exchange and routing key. Applied in every
    /// application - publishing is not restricted to the owning service.
    /// </summary>
    public static void ApplyPublishing(WolverineOptions options)
    {
        foreach (var route in Routes)
        {
            options.Publish(rule => rule
                .Message(route.MessageType)
                .ToRabbitRoutingKey(route.Exchange, route.RoutingKey, exchange => exchange.ExchangeType = ExchangeType.Direct));
        }
    }

    /// <summary>
    /// Binds the given queue to every exchange and routing key it consumes.
    /// </summary>
    public static void ApplyBindings(WolverineOptions options, string queueName)
    {
        foreach (var route in Routes.Where(o => o.Queue == queueName))
        {
            options.UseRabbitMq()
                .BindExchange(route.Exchange, exchange => exchange.ExchangeType = ExchangeType.Direct)
                .ToQueue(queueName, route.RoutingKey);
        }
    }
}
