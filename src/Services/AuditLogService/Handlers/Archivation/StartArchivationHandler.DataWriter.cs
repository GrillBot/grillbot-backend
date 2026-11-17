using System.Text.Json.Nodes;
using AuditLogService.Core.Entity;
using GrillBot.Contracts.AuditLog.Enums;
using Discord;

namespace AuditLogService.Handlers.Archivation;

public partial class StartArchivationHandler
{
    private static JsonObject? ProcessData(LogItem item, ArchiveData result)
    {
        return item.Type switch
        {
            LogType.Info or LogType.Warning or LogType.Error => CreateLogMessageItem(item),
            LogType.ChannelCreated => CreateChannelCreatedItem(item),
            LogType.ChannelUpdated => CreateChannelUpdatedItem(item),
            LogType.ChannelDeleted => CreateChannelDeletedItem(item),
            LogType.EmoteDeleted => CreateDeletedEmoteItem(item),
            LogType.OverwriteCreated => CreateOverwriteCreatedItem(item, result),
            LogType.OverwriteDeleted => CreateOverwriteDeletedItem(item, result),
            LogType.OverwriteUpdated => CreateOverwriteUpdatedItem(item, result),
            LogType.Unban => CreateUnbanItem(item, result),
            LogType.MemberUpdated => CreateMemberUpdatedItem(item, result),
            LogType.MemberRoleUpdated => CreateMemberRoleUpdatedItems(item, result),
            LogType.GuildUpdated => CreateGuildUpdatedItems(item, result),
            LogType.UserLeft => CreateUserLeftItem(item, result),
            LogType.UserJoined => CreateUserJoinedItem(item),
            LogType.MessageEdited => CreateMessageEditedItem(item),
            LogType.MessageDeleted => CreateMessageDeletedItem(item, result),
            LogType.InteractionCommand => CreateInteractionItem(item),
            LogType.ThreadDeleted => CreateThreadDeletedItem(item),
            LogType.JobCompleted => CreateJobCompletedItem(item, result),
            LogType.Api => CreateApiRequestItem(item),
            LogType.ThreadUpdated => CreateThreadUpdatedItem(item),
            LogType.RoleDeleted => CreateRoleDeletedItem(item),
            LogType.Identity => CreateIdentityEventItem(item),
            LogType.Archivation => CreateArchivationItem(item),
            _ => null
        };
    }

    private static JsonObject? CreateLogMessageItem(LogItem item)
    {
        if (item.LogMessage is null)
            return null;

        return new JsonObject
        {
            ["AppName"] = item.LogMessage.SourceAppName,
            ["Source"] = item.LogMessage.Source,
            ["Severity"] = item.LogMessage.Severity.ToString(),
            ["Message"] = item.LogMessage.Message
        };
    }

    private static JsonObject? CreateChannelCreatedItem(LogItem item)
        => item.ChannelCreated is null ? null : new JsonObject { ["ChannelInfo"] = CreateChannelInfoItems(item.ChannelCreated.ChannelInfo) };

    private static JsonObject? CreateChannelUpdatedItem(LogItem item)
    {
        if (item.ChannelUpdated is null)
            return null;

        return new JsonObject
        {
            ["Before"] = CreateChannelInfoItems(item.ChannelUpdated.Before),
            ["After"] = CreateChannelInfoItems(item.ChannelUpdated.After)
        };
    }

    private static JsonObject? CreateChannelDeletedItem(LogItem item)
        => item.ChannelDeleted is null ? null : new JsonObject { ["ChannelInfo"] = CreateChannelInfoItems(item.ChannelDeleted.ChannelInfo) };

    private static JsonObject CreateChannelInfoItems(ChannelInfo info)
    {
        var json = new JsonObject
        {
            ["Position"] = info.Position,
            ["Flags"] = info.Flags
        };

        if (!string.IsNullOrEmpty(info.ChannelName))
            json["Name"] = info.ChannelName;

        if (info.SlowMode is not null)
            json["Slowmode"] = info.SlowMode.Value;

        if (info.ChannelType is not null)
            json["Type"] = info.ChannelType.Value.ToString();

        if (info.IsNsfw is not null)
            json["IsNsfw"] = info.IsNsfw.Value;

        if (info.Bitrate is not null)
            json["Bitrate"] = info.Bitrate.Value;

        return json;
    }

    private static JsonObject? CreateDeletedEmoteItem(LogItem item)
    {
        if (item.DeletedEmote is null)
            return null;

        return new JsonObject
        {
            ["Id"] = item.DeletedEmote.EmoteId,
            ["Name"] = item.DeletedEmote.EmoteName
        };
    }

    private static JsonObject? CreateOverwriteCreatedItem(LogItem item, ArchiveData result)
        => item.OverwriteCreated is null ? null : new JsonObject { ["OverwriteInfo"] = CreateOverwriteInfoItems(item.OverwriteCreated.OverwriteInfo, result) };

    private static JsonObject? CreateOverwriteDeletedItem(LogItem item, ArchiveData result)
        => item.OverwriteDeleted is null ? null : new JsonObject { ["OverwriteInfo"] = CreateOverwriteInfoItems(item.OverwriteDeleted.OverwriteInfo, result) };

    private static JsonObject? CreateOverwriteUpdatedItem(LogItem item, ArchiveData result)
    {
        if (item.OverwriteUpdated is null)
            return null;

        return new JsonObject
        {
            ["Before"] = CreateOverwriteInfoItems(item.OverwriteUpdated.Before, result),
            ["After"] = CreateOverwriteInfoItems(item.OverwriteUpdated.After, result)
        };
    }

    private static JsonObject CreateOverwriteInfoItems(OverwriteInfo info, ArchiveData result)
    {
        var json = new JsonObject
        {
            ["TargetType"] = info.Target.ToString(),
            ["TargetId"] = info.TargetId
        };

        if (info.Target == PermissionTarget.User && !result.UserIds.Contains(info.TargetId))
            result.UserIds.Add(info.TargetId);

        var perms = new OverwritePermissions(info.AllowValue, info.DenyValue);
        if (perms.AllowValue > 0)
            json["Allow"] = new JsonArray([.. perms.ToAllowList().Select(o => JsonValue.Create(o.ToString()))]);
        if (perms.DenyValue > 0)
            json["Deny"] = new JsonArray([.. perms.ToDenyList().Select(o => JsonValue.Create(o.ToString()))]);

        return json;
    }

    private static JsonObject? CreateUnbanItem(LogItem item, ArchiveData result)
    {
        if (item.Unban is null)
            return null;

        if (!result.UserIds.Contains(item.Unban.UserId))
            result.UserIds.Add(item.Unban.UserId);

        return new JsonObject
        {
            ["UserId"] = item.Unban.UserId
        };
    }

    private static JsonObject? CreateMemberUpdatedItem(LogItem item, ArchiveData result)
    {
        if (item.MemberUpdated is null)
            return null;

        return new JsonObject
        {
            ["Before"] = CreateMemberInfoItems(item.MemberUpdated.Before, result),
            ["After"] = CreateMemberInfoItems(item.MemberUpdated.After, result)
        };
    }

    private static JsonObject CreateMemberInfoItems(MemberInfo info, ArchiveData result)
    {
        var json = new JsonObject
        {
            ["UserId"] = info.UserId
        };

        if (!result.UserIds.Contains(info.UserId))
            result.UserIds.Add(info.UserId);

        if (!string.IsNullOrEmpty(info.Nickname))
            json["Nickname"] = info.Nickname;
        if (info.IsMuted is not null)
            json["IsMuted"] = info.IsMuted.Value;
        if (info.IsDeaf is not null)
            json["IsDeaf"] = info.IsDeaf.Value;
        if (!string.IsNullOrEmpty(info.SelfUnverifyMinimalTime))
            json["SelfUnverifyMinimalTime"] = info.SelfUnverifyMinimalTime;
        if (info.Flags is not null)
            json["Flags"] = info.Flags.Value;
        if (info.PointsDeactivated is not null)
            json["PointsDeactivated"] = info.PointsDeactivated.Value;

        return json;
    }

    private static JsonObject? CreateMemberRoleUpdatedItems(LogItem item, ArchiveData result)
    {
        if (item.MemberRolesUpdated is null || item.MemberRolesUpdated.Count == 0)
            return null;

        var roles = new JsonArray();
        foreach (var role in item.MemberRolesUpdated)
        {
            if (!result.UserIds.Contains(role.UserId))
                result.UserIds.Add(role.UserId);

            roles.Add(new JsonObject
            {
                ["UserId"] = role.UserId,
                ["RoleId"] = role.RoleId,
                ["RoleName"] = role.RoleName,
                ["RoleColor"] = role.RoleColor,
                ["IsAdded"] = role.IsAdded
            });
        }

        return new JsonObject
        {
            ["Roles"] = roles
        };
    }

    private static JsonObject? CreateGuildUpdatedItems(LogItem item, ArchiveData result)
    {
        if (item.GuildUpdated is null)
            return null;

        return new JsonObject
        {
            ["Before"] = CreateGuildUpdatedItems(item.GuildUpdated.Before, result),
            ["After"] = CreateGuildUpdatedItems(item.GuildUpdated.After, result)
        };
    }

    private static JsonObject CreateGuildUpdatedItems(GuildInfo info, ArchiveData result)
    {
        var json = new JsonObject
        {
            ["DefaultMessageNotifications"] = info.DefaultMessageNotifications.ToString(),
            ["AfkTimeout"] = info.AfkTimeout,
            ["Name"] = info.Name,
            ["MfaLevel"] = info.MfaLevel.ToString(),
            ["VerificationLevel"] = info.VerificationLevel.ToString(),
            ["ExplicitContentFilter"] = info.ExplicitContentFilter.ToString(),
            ["PremiumTier"] = info.PremiumTier.ToString(),
            ["SystemChannelFlags"] = info.SystemChannelFlags.ToString(),
            ["NsfwLevel"] = info.NsfwLevel.ToString()
        };

        SetIfNotEmpty(json, "Description", info.Description);
        SetIfNotEmpty(json, "VanityUrl", info.VanityUrl);
        SetIfNotEmpty(json, "BannerId", info.BannerId);
        SetIfNotEmpty(json, "DiscoverySplashId", info.DiscoverySplashId);
        SetIfNotEmpty(json, "SplashId", info.SplashId);
        SetIfNotEmpty(json, "IconId", info.IconId);

        if (info.IconData is not null)
            json["IconData"] = Convert.ToBase64String(info.IconData);

        SetChannelIfNotEmpty(json, "PublicUpdatesChannelId", info.PublicUpdatesChannelId, result);
        SetChannelIfNotEmpty(json, "RulesChannelId", info.RulesChannelId, result);
        SetChannelIfNotEmpty(json, "SystemChannelId", info.SystemChannelId, result);
        SetChannelIfNotEmpty(json, "AfkChannelId", info.AfkChannelId, result);

        var features = Enum.GetValues<GuildFeature>()
            .Where(f => (info.Features & f) != GuildFeature.None)
            .Select(value => value.ToString())
            .ToList();
        if (features.Count > 0)
            json["Features"] = string.Join(", ", features);

        return json;
    }

    private static void SetIfNotEmpty(JsonObject json, string propertyName, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            json[propertyName] = value;
    }

    /// <summary>Like <see cref="SetIfNotEmpty"/>, and records the channel so the bot adds its details to the archive.</summary>
    private static void SetChannelIfNotEmpty(JsonObject json, string propertyName, string? channelId, ArchiveData result)
    {
        if (string.IsNullOrEmpty(channelId))
            return;

        result.ChannelIds.Add(channelId);
        json[propertyName] = channelId;
    }

    private static JsonObject? CreateUserLeftItem(LogItem item, ArchiveData result)
    {
        if (item.UserLeft is null)
            return null;

        if (!result.UserIds.Contains(item.UserLeft.UserId))
            result.UserIds.Add(item.UserLeft.UserId);

        var json = new JsonObject
        {
            ["UserId"] = item.UserLeft.UserId,
            ["MemberCount"] = item.UserLeft.MemberCount,
            ["IsBan"] = item.UserLeft.IsBan
        };

        if (!string.IsNullOrEmpty(item.UserLeft.BanReason))
            json["BanReason"] = item.UserLeft.BanReason;
        return json;
    }

    private static JsonObject? CreateUserJoinedItem(LogItem item)
    {
        if (item.UserJoined is null)
            return null;

        return new JsonObject
        {
            ["MemberCount"] = item.UserJoined.MemberCount
        };
    }

    private static JsonObject? CreateMessageEditedItem(LogItem item)
    {
        if (item.MessageEdited is null)
            return null;

        return new JsonObject
        {
            ["JumpUrl"] = item.MessageEdited.JumpUrl,
            ["Before"] = item.MessageEdited.ContentBefore,
            ["After"] = item.MessageEdited.ContentAfter
        };
    }

    private static JsonObject? CreateMessageDeletedItem(LogItem item, ArchiveData result)
    {
        if (item.MessageDeleted is null)
            return null;

        result.UserIds.Add(item.MessageDeleted.AuthorId);

        var json = new JsonObject
        {
            ["AuthorId"] = item.MessageDeleted.AuthorId,
            ["MessageCreatedAt"] = item.MessageDeleted.MessageCreatedAt
        };

        SetIfNotEmpty(json, "Content", item.MessageDeleted.Content);

        json["Embeds"] = new JsonArray([.. item.MessageDeleted.Embeds.Select(CreateEmbedItem)]);
        return json;
    }

    private static JsonObject CreateEmbedItem(EmbedInfo embed)
    {
        var json = new JsonObject
        {
            ["Type"] = embed.Type,
            ["ContainsFooter"] = embed.ContainsFooter
        };

        SetIfNotEmpty(json, "Title", embed.Title);
        SetIfNotEmpty(json, "ImageInfo", embed.ImageInfo);
        SetIfNotEmpty(json, "VideoInfo", embed.VideoInfo);
        SetIfNotEmpty(json, "Author", embed.AuthorName);
        SetIfNotEmpty(json, "Provider", embed.ProviderName);
        SetIfNotEmpty(json, "ThumbnailInfo", embed.ThumbnailInfo);

        if (embed.Fields.Count > 0)
        {
            json["Fields"] = new JsonArray(
                [.. embed.Fields.Select(f => new JsonObject
                {
                    ["Name"] = f.Name,
                    ["Value"] = f.Value,
                    ["Inline"] = f.Inline
                })]
            );
        }

        return json;
    }

    private static JsonObject? CreateInteractionItem(LogItem item)
    {
        if (item.InteractionCommand is null)
            return null;

        var json = new JsonObject
        {
            ["Name"] = item.InteractionCommand.Name,
            ["Module"] = item.InteractionCommand.ModuleName,
            ["Method"] = item.InteractionCommand.MethodName,
            ["HasResponded"] = item.InteractionCommand.HasResponded,
            ["IsValidToken"] = item.InteractionCommand.IsValidToken,
            ["IsSuccess"] = item.InteractionCommand.IsSuccess,
            ["Duration"] = item.InteractionCommand.Duration,
            ["Locale"] = item.InteractionCommand.Locale,
            ["EndAt"] = item.InteractionCommand.EndAt,
            ["InteractionDate"] = item.InteractionCommand.InteractionDate.ToString("yyyy-MM-dd")
        };

        if (item.InteractionCommand.Parameters?.Count > 0)
        {
            var parameters = new JsonArray();
            foreach (var parameter in item.InteractionCommand.Parameters)
            {
                var parametersJson = new JsonObject
                {
                    ["Name"] = parameter.Name,
                    ["Type"] = parameter.Type
                };

                if (!string.IsNullOrEmpty(parameter.Value))
                    parametersJson["Value"] = parameter.Value;

                parameters.Add(parametersJson);
            }

            json["Parameters"] = parameters;
        }

        if (item.InteractionCommand.CommandError is not null)
            json["CommandError"] = item.InteractionCommand.CommandError.Value;
        if (!string.IsNullOrEmpty(item.InteractionCommand.ErrorReason))
            json["ErrorReason"] = item.InteractionCommand.ErrorReason;
        if (!string.IsNullOrEmpty(item.InteractionCommand.Exception))
            json["Exception"] = item.InteractionCommand.Exception;
        return json;
    }

    private static JsonObject? CreateThreadDeletedItem(LogItem item)
    {
        if (item.ThreadDeleted is null)
            return null;

        return new JsonObject
        {
            ["ThreadInfo"] = CreateThreadInfoItems(item.ThreadDeleted.ThreadInfo)
        };
    }

    private static JsonObject CreateThreadInfoItems(ThreadInfo info)
    {
        var json = new JsonObject
        {
            ["Name"] = info.ThreadName,
            ["Type"] = info.Type.ToString(),
            ["IsArchived"] = info.IsArchived,
            ["ArchiveDuration"] = info.ArchiveDuration.ToString(),
            ["IsLocked"] = info.IsLocked
        };

        if (info.SlowMode is not null)
            json["SlowMode"] = info.SlowMode.Value;

        if (info.Tags?.Count > 0)
            json["Tags"] = new JsonArray([.. info.Tags.Select(t => JsonValue.Create(t))]);
        return json;
    }

    private static JsonObject? CreateJobCompletedItem(LogItem item, ArchiveData result)
    {
        if (item.Job is null)
            return null;

        var json = new JsonObject
        {
            ["JobName"] = item.Job.JobName,
            ["Result"] = item.Job.Result,
            ["StartAt"] = item.Job.StartAt,
            ["EndAt"] = item.Job.EndAt,
            ["WasError"] = item.Job.WasError,
            ["Duration"] = item.Job.Duration,
            ["JobDate"] = item.Job.JobDate.ToString("yyyy-MM-dd")
        };

        if (string.IsNullOrEmpty(item.Job.StartUserId))
            return json;

        if (!result.UserIds.Contains(item.Job.StartUserId))
            result.UserIds.Add(item.Job.StartUserId);

        json["StartUserId"] = item.Job.StartUserId;
        return json;
    }

    private static JsonObject? CreateApiRequestItem(LogItem item)
    {
        if (item.ApiRequest is null)
            return null;

        var json = new JsonObject
        {
            ["Controller"] = item.ApiRequest.ControllerName,
            ["Action"] = item.ApiRequest.ActionName,
            ["StartAt"] = item.ApiRequest.StartAt,
            ["EndAt"] = item.ApiRequest.EndAt,
            ["Method"] = item.ApiRequest.Method,
            ["TemplatePath"] = item.ApiRequest.TemplatePath,
            ["Path"] = item.ApiRequest.Path,
            ["Language"] = item.ApiRequest.Language,
            ["ApiGroup"] = item.ApiRequest.ApiGroupName,
            ["Identification"] = item.ApiRequest.Identification,
            ["Ip"] = item.ApiRequest.Ip,
            ["Result"] = item.ApiRequest.Result,
            ["IsSuccess"] = item.ApiRequest.IsSuccess,
            ["Duration"] = item.ApiRequest.Duration
        };

        if (item.ApiRequest.Parameters?.Count > 0)
        {
            json["Parameters"] = new JsonObject(
                item.ApiRequest.Parameters.Select(o => KeyValuePair.Create<string, JsonNode?>(o.Key, JsonValue.Create(o.Value)))
            );
        }

        if (item.ApiRequest.Headers?.Count > 0)
        {
            json["Headers"] = new JsonObject(
                item.ApiRequest.Headers.Select(o => KeyValuePair.Create<string, JsonNode?>(o.Key, JsonValue.Create(o.Value)))
            );
        }

        if (!string.IsNullOrEmpty(item.ApiRequest.Role))
            json["Role"] = item.ApiRequest.Role;

        if (!string.IsNullOrEmpty(item.ApiRequest.ForwardedIp))
            json["ForwardedIp"] = item.ApiRequest.ForwardedIp;

        return json;
    }

    private static JsonObject? CreateThreadUpdatedItem(LogItem item)
    {
        if (item.ThreadUpdated is null)
            return null;

        return new JsonObject
        {
            ["Before"] = CreateThreadInfoItems(item.ThreadUpdated.Before),
            ["After"] = CreateThreadInfoItems(item.ThreadUpdated.After)
        };
    }

    private static JsonObject? CreateRoleDeletedItem(LogItem item)
    {
        if (item.RoleDeleted?.RoleInfo is null)
            return null;

        return new JsonObject
        {
            ["Role"] = CreateRoleInfoItems(item.RoleDeleted.RoleInfo)
        };
    }

    private static JsonObject? CreateIdentityEventItem(LogItem item)
    {
        if (item.IdentityEvent is null)
            return null;

        var json = new JsonObject
        {
            ["EventType"] = item.IdentityEvent.EventType.ToString(),
            ["Channel"] = item.IdentityEvent.Channel
        };

        if (!string.IsNullOrEmpty(item.IdentityEvent.Reason))
            json["Reason"] = item.IdentityEvent.Reason;
        if (!string.IsNullOrEmpty(item.IdentityEvent.IpAddress))
            json["IpAddress"] = item.IdentityEvent.IpAddress;

        return json;
    }

    private static JsonObject? CreateArchivationItem(LogItem item)
    {
        if (item.Archivation is null)
            return null;

        var json = new JsonObject
        {
            ["ProcessName"] = item.Archivation.ProcessName,
            ["TotalRecordsCount"] = item.Archivation.TotalRecordsCount,
            ["JsonSize"] = item.Archivation.JsonSize,
            ["ArchiveSize"] = item.Archivation.ArchiveSize
        };

        if (!string.IsNullOrEmpty(item.Archivation.Details))
            json["Details"] = item.Archivation.Details;
        if (item.Archivation.DeletedFilesCount is not null)
            json["DeletedFilesCount"] = item.Archivation.DeletedFilesCount;
        if (item.Archivation.DeletedFilesSize is not null)
            json["DeletedFilesSize"] = item.Archivation.DeletedFilesSize;

        return json;
    }

    private static JsonObject CreateRoleInfoItems(RoleInfo info)
    {
        var json = new JsonObject
        {
            ["RoleId"] = info.RoleId,
            ["Name"] = info.Name,
            ["IsMentionable"] = info.IsMentionable,
            ["IsHoisted"] = info.IsHoisted
        };

        if (!string.IsNullOrEmpty(info.Color))
            json["Color"] = info.Color;

        if (info.Permissions?.Count > 0)
        {
            json["Permissions"] = new JsonArray(
                [.. info.Permissions.Select(p => JsonValue.Create(p))]
            );
        }

        if (!string.IsNullOrEmpty(info.IconId))
            json["IconId"] = info.IconId;
        return json;
    }
}
