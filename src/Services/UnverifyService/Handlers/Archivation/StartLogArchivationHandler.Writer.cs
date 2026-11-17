using System.Text.Json.Nodes;
using GrillBot.Contracts.Unverify.Events;
using GrillBot.Core.IO;
using UnverifyService.Core.Entity.Logs;

namespace UnverifyService.Handlers.Archivation;

public partial class StartLogArchivationHandler
{
    /// <summary>What the archive references, collected while the records are written.</summary>
    private sealed class ArchiveData
    {
        public List<Guid> Ids { get; } = [];
        public HashSet<ulong> UserIds { get; } = [];
        public HashSet<ulong> GuildIds { get; } = [];
        public HashSet<ulong> ChannelIds { get; } = [];
        public Dictionary<string, int> PerType { get; } = [];
        public Dictionary<string, int> PerMonths { get; } = [];
    }

    /// <summary>
    /// Writes <c>{ CreatedAt, Items: [...], Count }</c>. Each record is turned into a small JSON object,
    /// written to the compressed stream and forgotten.
    /// </summary>
    private static async Task<UnverifyLogArchiveDataPreparedEvent> CreateArchiveAsync(IAsyncEnumerable<UnverifyLogItem> items, CancellationToken cancellationToken)
    {
        var data = new ArchiveData();
        var createdAt = DateTime.UtcNow;

        await using var json = new CompressedJsonWriter();
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

        var content = await json.CompleteAsync(cancellationToken);

        return new UnverifyLogArchiveDataPreparedEvent
        {
            CreatedAtUtc = createdAt,
            CompressedContent = content,
            JsonSize = json.UncompressedSize,
            ItemsCount = data.Ids.Count,
            Ids = data.Ids,
            UserIds = [.. data.UserIds],
            GuildIds = [.. data.GuildIds],
            ChannelIds = [.. data.ChannelIds],
            PerType = data.PerType.OrderBy(o => o.Key).ToDictionary(o => o.Key, o => o.Value),
            PerMonths = data.PerMonths
        };
    }

    private static JsonObject ProcessLogItem(UnverifyLogItem item, ArchiveData result)
    {
        result.Ids.Add(item.Id);

        var json = new JsonObject
        {
            ["Id"] = item.Id,
            ["Number"] = item.LogNumber,
            ["Type"] = item.OperationType.ToString(),
            ["CreatedAt"] = item.CreatedAt,
            ["GuildId"] = item.GuildId.Value,
            ["FromUserId"] = item.FromUserId.Value,
            ["ToUserId"] = item.ToUserId.Value
        };

        result.GuildIds.Add(item.GuildId);
        result.UserIds.Add(item.FromUserId);
        result.UserIds.Add(item.ToUserId);

        if (item.ParentLogItemId is not null)
            json["ParentId"] = item.ParentLogItemId;

        var dataJson = ProcessData(item, result);
        if (dataJson is not null)
            json["Data"] = dataJson;

        IncrementStats(item.OperationType.ToString(), result.PerType);
        IncrementStats(item.CreatedAt.ToString("MM-yyyy"), result.PerMonths);
        return json;
    }

    private static void IncrementStats(string key, Dictionary<string, int> stats)
        => stats[key] = stats.GetValueOrDefault(key) + 1;
}
