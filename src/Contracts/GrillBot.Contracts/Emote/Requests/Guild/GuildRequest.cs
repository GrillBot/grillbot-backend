using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;

namespace GrillBot.Contracts.Emote.Requests.Guild;

public class GuildRequest : IDictionaryObject
{
    [DiscordId]
    public string? SuggestionChannelId { get; set; }

    [DiscordId]
    public string? VoteChannelId { get; set; }

    public TimeSpan VoteTime { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>()
        {
            { nameof(SuggestionChannelId), SuggestionChannelId },
            { nameof(VoteChannelId), VoteChannelId },
            { nameof(VoteTime), VoteTime.ToString("c") }
        };
    }
}
