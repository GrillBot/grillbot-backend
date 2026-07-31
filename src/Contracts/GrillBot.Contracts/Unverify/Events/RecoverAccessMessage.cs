namespace GrillBot.Contracts.Unverify.Events;

public sealed record RecoverAccessMessage(long? LogNumber, Guid? LogId);
