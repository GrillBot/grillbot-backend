namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Asks the unverify service to archive log records whose retention expired.
/// When nothing qualifies the service does nothing; otherwise it answers with
/// <see cref="UnverifyLogArchiveDataPreparedEvent"/>.
/// </summary>
public sealed record StartUnverifyLogArchivationCommand
{
    /// <summary>
    /// Position of this run in a chain: 0 for the scheduled run, then +1 for every run the service starts by
    /// itself after <see cref="DeleteUnverifyLogRecordsCommand"/> while expired records remain.
    /// </summary>
    public int Sequence { get; init; }
}
