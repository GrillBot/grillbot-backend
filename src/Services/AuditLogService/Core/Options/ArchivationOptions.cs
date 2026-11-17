namespace AuditLogService.Core.Options;

public class ArchivationOptions
{
    /// <summary>Retention of records no rule in <c>AuditLogRetention</c> matches.</summary>
    public int DefaultRetentionMonths { get; set; } = 12;

    /// <summary>Retention per rule name of <c>AuditLogRetention.Rules</c>. Every rule must have a value.</summary>
    public Dictionary<string, int> RetentionMonths { get; set; } = [];

    /// <summary>A run archives nothing until at least this many records expired.</summary>
    public int MinimalItemsToArchive { get; set; }

    /// <summary>Upper bound of records in one archive (one run).</summary>
    public int MaxItemsToArchivePerRun { get; set; } = 2500;

    /// <summary>How many records are read from the database at once while the archive is built.</summary>
    public int ReadPageSize { get; set; } = 100;

    /// <summary>Compressed archives bigger than this are still published, but logged as a warning (broker message size limit).</summary>
    public long CompressedSizeWarningThreshold { get; set; } = 10 * 1024 * 1024;
}
