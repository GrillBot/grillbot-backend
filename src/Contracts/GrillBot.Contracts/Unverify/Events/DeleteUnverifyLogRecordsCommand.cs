using System.Text.Json.Serialization;

namespace GrillBot.Contracts.Unverify.Events;

/// <summary>
/// Deletes unverify log records (with their operations). Records of an active unverify are kept.
/// </summary>
[method: JsonConstructor]
public sealed record DeleteUnverifyLogRecordsCommand(List<Guid> Ids)
{
    public DeleteUnverifyLogRecordsCommand(Guid id) : this([id])
    {
    }

    /// <summary>
    /// Set when the records come from an archivation run (its <see cref="StartUnverifyLogArchivationCommand.Sequence"/>)
    /// and more expired records remain: after the delete the service starts the next run. The next run is started
    /// from here, not by the bot, because only the same queue guarantees it runs after this delete.
    /// </summary>
    public int? ContinueArchivationSequence { get; init; }
}
