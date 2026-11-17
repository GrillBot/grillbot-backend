namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// The result of <see cref="StartAuditLogArchivationCommand"/>: archive data the bot enriches,
/// zips (with attachments) and stores before deleting the records with <see cref="DeleteAuditLogRecordsCommand"/>.
/// </summary>
public sealed record AuditLogArchiveDataPreparedEvent
{
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Brotli-compressed UTF-8 JSON of the archived records.</summary>
    public byte[] CompressedContent { get; init; } = [];

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
