namespace GrillBot.Contracts.Bot.Events.Messages;

public sealed record CreatedDiscordMessagePayload(
    string? GuildId,
    string ChannelId,
    string MessageId,
    string ServiceId,
    Dictionary<string, string> ServiceData
);
