namespace GrillBot.Contracts.AuditLog.Events;

public sealed record BulkDeletePayload(List<Guid> Ids);
