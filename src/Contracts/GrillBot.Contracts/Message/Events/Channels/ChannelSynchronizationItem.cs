using Discord;

namespace GrillBot.Contracts.Message.Events.Channels;

public sealed record ChannelSynchronizationItem(
    ulong GuildId,
    ulong ChannelId,
    bool? IsDeleted = null,
    bool? IsPointsDisabled = null,
    bool? IsAutoReplyDisabled = null
)
{
    public static ChannelSynchronizationItem FromChannel(IGuildChannel channel)
        => new(channel.GuildId, channel.Id);
}
