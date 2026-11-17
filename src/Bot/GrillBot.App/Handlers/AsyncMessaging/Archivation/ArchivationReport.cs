using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Builds the <see cref="LogType.Archivation"/> audit log record of a finished run.
/// </summary>
public static class ArchivationReport
{
    private static readonly string Indent = new(' ', 5);

    public static CreateItemsMessage CreateLogMessage(ArchivationRequest request)
        => new(new LogRequest(LogType.Archivation, DateTime.UtcNow) { Archivation = request });

    public static string BuildDetails(
        string archiveName,
        IEnumerable<KeyValuePair<string, long>> perType,
        IEnumerable<KeyValuePair<string, long>> perMonths,
        IEnumerable<string>? notes = null
    )
    {
        var builder = new StringBuilder()
            .Append("Archive: ").AppendLine(archiveName);

        foreach (var note in notes ?? [])
            builder.AppendLine(note);

        builder.AppendLine().AppendLine("Archived types: (");
        foreach (var type in perType)
            builder.Append(Indent).Append(type.Key).Append(": ").Append(type.Value).AppendLine();
        builder.AppendLine(")");

        builder.AppendLine("Archived months: (");
        foreach (var month in perMonths)
            builder.Append(Indent).Append(month.Key).Append(": ").Append(month.Value).AppendLine();
        builder.Append(')');

        return builder.ToString();
    }
}
