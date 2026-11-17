namespace GrillBot.Contracts.AuditLog.Events;

/// <summary>
/// Deletes audit log records (with their child data and attached files).
/// </summary>
public sealed record DeleteAuditLogRecordsCommand(List<Guid> Ids);
