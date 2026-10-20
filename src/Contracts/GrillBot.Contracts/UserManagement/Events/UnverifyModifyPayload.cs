namespace GrillBot.Contracts.UserManagement.Events;

public sealed record UnverifyModifyPayload(long LogSetId, DateTime? NewEndUtc);
