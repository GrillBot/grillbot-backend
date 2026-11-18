namespace UnverifyService.Options;

public class ArchivationOptions
{
    /// <summary>A run archives nothing until at least this many records expired.</summary>
    public int MinimalCount { get; set; }

    /// <summary>Retention of unverify log records.</summary>
    public TimeSpan ExpirationMilestone { get; set; }

    /// <summary>Upper bound of records in one archive (one run).</summary>
    public int MaxItemsPerRun { get; set; } = 2500;

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
}
