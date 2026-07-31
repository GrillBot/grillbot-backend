namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Published and consumed by the unverify service itself to refresh its metrics
/// off the request thread.
/// </summary>
public sealed record RecalculateMetricsMessage;
