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

    /// <summary>
    /// How many runs one night may chain: after the delete of an archive, the next run starts at once while expired
    /// records remain. The rest waits for the next scheduled run.
    /// </summary>
    public int MaxChainedRuns { get; set; } = 20;

    /// <summary>
    /// Archive files in the shared storage older than this are deleted at the start of a run: leftovers of crashed
    /// runs and of events that were never consumed.
    /// </summary>
    public TimeSpan SharedFileRetention { get; set; } = TimeSpan.FromDays(3);

    /// <summary>
    /// How often the count of records to archive (the <c>items_to_archive</c> metric) is recalculated.
    /// Records age into the retention window continuously, so the count grows between archivation runs.
    /// </summary>
    public TimeSpan TelemetryRefreshInterval { get; set; } = TimeSpan.FromMinutes(15);
}
