using GrillBot.Core.Infrastructure;

namespace UnverifyService.Models.Request;

public class UnverifyRequest : IDictionaryObject
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }

    public DateTime EndAtUtc { get; set; }
    public string? Reason { get; set; }
    public bool TestRun { get; set; }
    public bool IsSelfUnverify { get; set; }
    public List<string> RequiredKeepables { get; set; } = [];

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId.ToString() },
            { nameof(UserId), UserId.ToString() },
            { nameof(ChannelId), ChannelId.ToString() },
            { nameof(MessageId), MessageId.ToString() },
            { nameof(EndAtUtc), EndAtUtc.ToString("o") },
            { nameof(Reason), Reason },
            { nameof(TestRun), TestRun.ToString() },
            { nameof(IsSelfUnverify), IsSelfUnverify.ToString() }
        };

        for (var i = 0; i < RequiredKeepables.Count; i++)
            result[$"{nameof(RequiredKeepables)}[{i}]"] = RequiredKeepables[i];

        return result;
    }
}
