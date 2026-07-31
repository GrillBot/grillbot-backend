namespace GrillBot.Contracts.UserMeasures.Events;

public sealed record UnverifyModifyPayload(long LogSetId, DateTime? NewEndUtc);
