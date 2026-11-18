namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// The result of <see cref="StartUnverifyLogArchivationCommand"/>: archive data the bot enriches,
/// zips and stores before deleting the records with <see cref="DeleteUnverifyLogRecordsCommand"/>.
/// </summary>
public sealed record UnverifyLogArchiveDataPreparedEvent
{
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Name of the Brotli-compressed UTF-8 JSON of the archived records in the shared storage
    /// (<c>SharedStorage:Path</c>). A plain file name, never a path; the consumer deletes the file when done.
    /// </summary>
    public string ContentFileName { get; init; } = "";

    /// <summary>Size of the compressed file in bytes.</summary>
    public long CompressedSize { get; init; }

    /// <summary>Position of the run in a chain, copied from the start command.</summary>
    public int Sequence { get; init; }

    /// <summary>More expired records remained than this archive holds; the delete command continues the chain.</summary>
    public bool HasMore { get; init; }

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
