using System.Text.Json.Nodes;
using UnverifyService.Core.Entity.Logs;
using GrillBot.Contracts.Unverify.Enums;

namespace UnverifyService.Handlers.Archivation;

public partial class StartLogArchivationHandler
{
    private static JsonObject? ProcessData(UnverifyLogItem item, ArchiveData result)
    {
        return item.OperationType switch
        {
            UnverifyOperationType.Update => CreateUnverifyUpdateItem(item),
            UnverifyOperationType.SelfUnverify or UnverifyOperationType.Unverify => CreateUnverifySetItem(item, result),
            UnverifyOperationType.ManualRemove or UnverifyOperationType.AutoRemove or UnverifyOperationType.Recovery => CreateUnverifyRemoveItem(item, result),
            _ => null
        };
    }

    private static JsonObject? CreateUnverifyUpdateItem(UnverifyLogItem item)
    {
        if (item.UpdateOperation is null)
            return null;

        var json = new JsonObject
        {
            ["NewStartAt"] = item.UpdateOperation.NewStartAtUtc,
            ["NewEndAt"] = item.UpdateOperation.NewEndAtUtc
        };

        if (!string.IsNullOrEmpty(item.UpdateOperation.Reason))
            json["Reason"] = item.UpdateOperation.Reason;

        return json;
    }

    private static JsonObject? CreateUnverifySetItem(UnverifyLogItem item, ArchiveData result)
    {
        if (item.SetOperation is null)
            return null;

        var json = new JsonObject
        {
            ["StartAt"] = item.SetOperation.StartAtUtc,
            ["EndAt"] = item.SetOperation.EndAtUtc,
            ["Language"] = item.SetOperation.Language,
            ["KeepMutedRole"] = item.SetOperation.KeepMutedRole
        };

        if (!string.IsNullOrEmpty(item.SetOperation.Reason))
            json["Reason"] = item.SetOperation.Reason;

        if (item.SetOperation.Roles.Count > 0)
        {
            json["Roles"] = new JsonObject(
                item.SetOperation.Roles.Select(o => new KeyValuePair<string, JsonNode?>(o.RoleId.ToString(), o.IsKept))
            );
        }

        if (item.SetOperation.Channels.Count > 0)
        {
            json["Channels"] = new JsonObject(
                item.SetOperation.Channels.Select(o => new KeyValuePair<string, JsonNode?>(
                    o.ChannelId.ToString(),
                    new JsonObject
                    {
                        ["AllowValue"] = o.AllowValue,
                        ["DenyValue"] = o.DenyValue,
                        ["IsKept"] = o.IsKept
                    }
                ))
            );

            result.ChannelIds.UnionWith(item.SetOperation.Channels.Select(o => o.ChannelId.Value));
        }

        return json;
    }

    private static JsonObject? CreateUnverifyRemoveItem(UnverifyLogItem item, ArchiveData result)
    {
        if (item.RemoveOperation is null)
            return null;

        var json = new JsonObject
        {
            ["IsFromWeb"] = item.RemoveOperation.IsFromWeb,
            ["Language"] = item.RemoveOperation.Language,
            ["Force"] = item.RemoveOperation.Force
        };

        if (item.RemoveOperation.Roles.Count > 0)
            json["Roles"] = new JsonArray([.. item.RemoveOperation.Roles.Select(o => o.RoleId.Value)]);

        if (item.RemoveOperation.Channels.Count > 0)
        {
            json["Channels"] = new JsonObject(
                item.RemoveOperation.Channels.Select(o => new KeyValuePair<string, JsonNode?>(
                    o.ChannelId.ToString(),
                    new JsonObject
                    {
                        ["AllowValue"] = o.AllowValue,
                        ["DenyValue"] = o.DenyValue
                    }
                ))
            );

            result.ChannelIds.UnionWith(item.RemoveOperation.Channels.Select(o => o.ChannelId.Value));
        }

        return json;
    }
}
