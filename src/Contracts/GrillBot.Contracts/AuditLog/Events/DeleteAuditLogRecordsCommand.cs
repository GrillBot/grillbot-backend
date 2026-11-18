namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// Deletes audit log records (with their child data and attached files).
/// </summary>
public sealed record DeleteAuditLogRecordsCommand(List<Guid> Ids)
{
    /// <summary>
    /// Set when the records come from an archivation run (its <see cref="StartAuditLogArchivationCommand.Sequence"/>)
    /// and more expired records remain: after the delete the service starts the next run. The next run is started
    /// from here, not by the bot, because only the same queue guarantees it runs after this delete.
    /// </summary>
    public int? ContinueArchivationSequence { get; init; }
}
