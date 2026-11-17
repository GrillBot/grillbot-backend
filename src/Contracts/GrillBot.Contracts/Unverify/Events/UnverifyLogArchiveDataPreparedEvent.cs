namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// The result of <see cref="StartUnverifyLogArchivationCommand"/>: archive data the bot enriches,
/// zips and stores before deleting the records with <see cref="DeleteUnverifyLogRecordsCommand"/>.
/// </summary>
public sealed record UnverifyLogArchiveDataPreparedEvent
{
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Brotli-compressed UTF-8 JSON of the archived records.</summary>
    public byte[] CompressedContent { get; init; } = [];

    /// <summary>Size of the uncompressed JSON in bytes.</summary>
    public long JsonSize { get; init; }

    public int ItemsCount { get; init; }
    public List<Guid> Ids { get; init; } = [];
    public List<ulong> UserIds { get; init; } = [];
    public List<ulong> GuildIds { get; init; } = [];
    public List<ulong> ChannelIds { get; init; } = [];
    public Dictionary<string, int> PerType { get; init; } = [];
    public Dictionary<string, int> PerMonths { get; init; } = [];
}
