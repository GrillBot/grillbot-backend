using System.Text.Json.Serialization;
using Discord;

namespace GrillBot.Contracts.Emote.Events;

[method: JsonConstructor]
public sealed record SynchronizeEmotesPayload(string GuildId, List<string> Emotes)
{
    public SynchronizeEmotesPayload(string guildId, IEnumerable<IEmote> emotes)
        : this(guildId, [.. emotes.Select(o => o.ToString()!)])
    {
    }
}
