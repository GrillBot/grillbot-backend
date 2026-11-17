namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// Asks the audit log service to archive records whose retention expired.
/// When nothing qualifies the service does nothing; otherwise it answers with
/// <see cref="AuditLogArchiveDataPreparedEvent"/>.
/// </summary>
public sealed record StartAuditLogArchivationCommand;
