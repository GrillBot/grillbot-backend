namespace GrillBot.Contracts.Emote.Events;

/// <summary>
/// Broker addresses for the emote bounded context.
/// </summary>
public static class MessagingConstants
{
    public const string Exchange = "emote";

    public static class RoutingKeys
    {
        public const string EmoteEvent = "emote-event";
        public const string GuildChannelDeleted = "guild-channel-deleted";
        public const string SuggestionApprovalChange = "suggestion-approval-change";
        public const string SuggestionCancelVote = "suggestion-cancel-vote";
        public const string SuggestionMessageCreated = "suggestion-message-created";
        public const string SuggestionMessageDeleted = "suggestion-message-deleted";
        public const string SuggestionRequest = "suggestion-request";
        public const string SuggestionUserVote = "suggestion-user-vote";
        public const string SuggestionVoteMessageCreated = "suggestion-vote-message-created";
        public const string Synchronization = "synchronization";
    }
}
