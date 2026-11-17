namespace GrillBot.Contracts.AuditLog.Events.Create;

/// <summary>
/// Report of one finished archivation run. Requests with <see cref="TotalRecordsCount"/> == 0 are not stored.
/// </summary>
public class ArchivationRequest
{
    /// <summary>Name of the archivation process, one of <see cref="ArchivationProcessNames"/>.</summary>
    public string ProcessName { get; set; } = null!;

    public int TotalRecordsCount { get; set; }

    /// <summary>Uncompressed, serialized size of the final JSON in bytes.</summary>
    public long JsonSize { get; set; }

    /// <summary>Compressed size of the final archive in bytes.</summary>
    public long ArchiveSize { get; set; }

    public string? Details { get; set; }

    /// <summary>Count of deleted files. Null when the process does not delete files.</summary>
    public int? DeletedFilesCount { get; set; }

    /// <summary>Size of deleted files in bytes. Null when the process does not delete files.</summary>
    public long? DeletedFilesSize { get; set; }
}

public static class ArchivationProcessNames
{
    public const string AuditLog = "AuditLog";
    public const string Unverify = "Unverify";
}
