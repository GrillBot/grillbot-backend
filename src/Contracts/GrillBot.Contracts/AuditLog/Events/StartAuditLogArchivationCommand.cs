namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// Asks the audit log service to archive records whose retention expired.
/// When nothing qualifies the service does nothing; otherwise it answers with
/// <see cref="AuditLogArchiveDataPreparedEvent"/>.
/// </summary>
public sealed record StartAuditLogArchivationCommand
{
    /// <summary>
    /// Position of this run in a chain: 0 for the scheduled run, then +1 for every run the service starts by
    /// itself after <see cref="DeleteAuditLogRecordsCommand"/> while expired records remain.
    /// </summary>
    public int Sequence { get; init; }
}
