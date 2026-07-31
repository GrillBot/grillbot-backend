using Discord;

namespace GrillBot.Contracts.Emote.Events.Guild;

public sealed record GuildChannelDeletedPayload(ulong GuildId, ulong ChannelId)
{
    public static GuildChannelDeletedPayload Create(IGuild guild, IChannel channel)
        => new(guild.Id, channel.Id);

    public static GuildChannelDeletedPayload Create(IGuildChannel channel)
        => new(channel.GuildId, channel.Id);
}
