namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Asks the unverify service to archive log records whose retention expired.
/// When nothing qualifies the service does nothing; otherwise it answers with
/// <see cref="UnverifyLogArchiveDataPreparedEvent"/>.
/// </summary>
public sealed record StartUnverifyLogArchivationCommand;
