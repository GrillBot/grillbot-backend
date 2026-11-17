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
}
