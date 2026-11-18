using System.Text.Json.Nodes;
using AuditLogService.Core.Entity;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Core.IO;
using File = AuditLogService.Core.Entity.File;

namespace AuditLogService.Handlers.Archivation;

public partial class StartArchivationHandler
{
    /// <summary>What the archive references, collected while the records are written.</summary>
    private sealed class ArchiveData
    {
        public List<Guid> Ids { get; } = [];
        public HashSet<string> Files { get; } = [];
        public HashSet<string> UserIds { get; } = [];
        public HashSet<string> GuildIds { get; } = [];
        public HashSet<string> ChannelIds { get; } = [];
        public long TotalFilesSize { get; set; }
        public Dictionary<string, long> PerType { get; } = [];
        public Dictionary<string, long> PerMonths { get; } = [];
    }

    /// <summary>
    /// Writes <c>{ CreatedAt, Items: [...], Count }</c> compressed into <paramref name="target"/>. Each record is turned
    /// into a small JSON object, written to the compressed stream and forgotten. The caller fills in the shared file name.
    /// </summary>
    private static async Task<AuditLogArchiveDataPreparedEvent> CreateArchiveAsync(IAsyncEnumerable<LogItem> items, Stream target, CancellationToken cancellationToken)
    {
        var data = new ArchiveData();
        var createdAt = DateTime.UtcNow;

        await using var json = new CompressedJsonWriter(target);
        var writer = json.Writer;

        writer.WriteStartObject();
        writer.WriteString("CreatedAt", createdAt);
        writer.WriteStartArray("Items");

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            ProcessLogItem(item, data).WriteTo(writer);
            await json.FlushIfNeededAsync(cancellationToken);
        }

        writer.WriteEndArray();
        writer.WriteNumber("Count", data.Ids.Count);
        writer.WriteEndObject();

        await json.CompleteAsync(cancellationToken);

        return new AuditLogArchiveDataPreparedEvent
        {
            CreatedAtUtc = createdAt,
            CompressedSize = json.CompressedSize,
            JsonSize = json.UncompressedSize,
            ItemsCount = data.Ids.Count,
            Ids = data.Ids,
            Files = [.. data.Files],
            TotalFilesSize = data.TotalFilesSize,
            UserIds = [.. data.UserIds],
            GuildIds = [.. data.GuildIds],
            ChannelIds = [.. data.ChannelIds],
            PerType = data.PerType.OrderBy(o => o.Key).ToDictionary(o => o.Key, o => o.Value),
            PerMonths = data.PerMonths
        };
    }

    private static JsonObject ProcessLogItem(LogItem item, ArchiveData result)
    {
        result.Ids.Add(item.Id);

        var json = new JsonObject
        {
            ["Id"] = item.Id,
            ["CreatedAt"] = item.CreatedAt,
            ["Type"] = item.Type.ToString(),
            ["IsDeleted"] = item.IsDeleted
        };

        if (!string.IsNullOrEmpty(item.GuildId))
        {
            result.GuildIds.Add(item.GuildId);
            json["GuildId"] = item.GuildId;
        }

        if (!string.IsNullOrEmpty(item.UserId))
        {
            result.UserIds.Add(item.UserId);
            json["UserId"] = item.UserId;
        }

        if (!string.IsNullOrEmpty(item.ChannelId))
        {
            result.ChannelIds.Add(item.ChannelId);
            json["ChannelId"] = item.ChannelId;
        }

        if (!string.IsNullOrEmpty(item.DiscordId))
            json["DiscordId"] = item.DiscordId;

        if (item.Files.Count > 0)
            json["Files"] = new JsonArray([.. item.Files.Select(o => ProcessFile(o, result))]);

        var dataJson = ProcessData(item, result);
        if (dataJson is not null)
            json["Data"] = dataJson;

        IncrementStats(item.Type.ToString(), result.PerType);
        IncrementStats(item.CreatedAt.ToString("MM-yyyy"), result.PerMonths);

        return json;
    }

    private static JsonObject ProcessFile(File file, ArchiveData result)
    {
        if (result.Files.Add(file.Filename))
            result.TotalFilesSize += file.Size;

        var json = new JsonObject
        {
            ["Filename"] = file.Filename,
            ["Size"] = file.Size
        };

        if (!string.IsNullOrEmpty(file.Extension))
            json["Extension"] = file.Extension;
        return json;
    }

    private static void IncrementStats(string key, Dictionary<string, long> stats)
        => stats[key] = stats.GetValueOrDefault(key) + 1;
}
