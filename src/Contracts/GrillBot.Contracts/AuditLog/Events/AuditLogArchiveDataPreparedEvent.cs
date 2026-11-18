namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// The result of <see cref="StartAuditLogArchivationCommand"/>: archive data the bot enriches,
/// zips (with attachments) and stores before deleting the records with <see cref="DeleteAuditLogRecordsCommand"/>.
/// </summary>
public sealed record AuditLogArchiveDataPreparedEvent
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
    public List<string> Files { get; init; } = [];
    public long TotalFilesSize { get; init; }
    public List<string> UserIds { get; init; } = [];
    public List<string> GuildIds { get; init; } = [];
    public List<string> ChannelIds { get; init; } = [];
    public Dictionary<string, long> PerType { get; init; } = [];
    public Dictionary<string, long> PerMonths { get; init; } = [];
}
