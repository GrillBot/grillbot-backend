using Discord;
using GrillBot.Core.RabbitMQ.V2.Messages;

namespace SearchingService.Models.Events;

public class SearchItemPayload : IRabbitMessage
{
    public string Topic => "Searching";
    public string Queue => "CreateSearchItem";

    public string UserId { get; set; } = null!;
    public string GuildId { get; set; } = null!;
    public string ChannelId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime? ValidToUtc { get; set; }

    public SearchItemPayload()
    {
    }

    public SearchItemPayload(string userId, string guildId, string channelId, string content, DateTime? validToUtc)
    {
        UserId = userId;
        GuildId = guildId;
        ChannelId = channelId;
        Content = content;
        ValidToUtc = validToUtc;
    }

    public SearchItemPayload(IUser user, IGuild guild, IChannel channel, string content, DateTime? validToUtc)
        : this(user.Id.ToString(), guild.Id.ToString(), channel.Id.ToString(), content, validToUtc)
    {
    }
}
