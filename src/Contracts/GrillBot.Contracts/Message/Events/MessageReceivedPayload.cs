using Discord;
using GrillBot.Contracts.Message.Events.Users;

namespace GrillBot.Contracts.Message.Events;

public sealed record MessageReceivedPayload(
    ulong Id,
    ulong ChannelId,
    ulong GuildId,
    UserPayload Author,
    MessageType Type,
    MessageSource Source,
    string? Content,
    DateTimeOffset CreatedAt
)
{
    public bool IsCommand()
        => Type is MessageType.ApplicationCommand or MessageType.ContextMenuCommand;

    public static MessageReceivedPayload? Create(IMessage message)
    {
        if (message.Channel is not IGuildChannel guildChannel)
            return null;

        return new MessageReceivedPayload(
            message.Id,
            message.Channel.Id,
            guildChannel.GuildId,
            UserPayload.Create(message.Author),
            message.Type,
            message.Source,
            message.Content,
            message.CreatedAt
        );
    }
}
