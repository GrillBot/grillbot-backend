using System.Text.Json.Nodes;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.AsyncMessaging.Archivation;

/// <summary>
/// Adds what the services do not know to an archive: the guilds, channels and users it references,
/// read from the bot's database.
/// </summary>
public static class ArchiveEnricher
{
    private const int ChunkSize = 100;

    public static async Task EnrichAsync(
        GrillBotRepository repository,
        JsonObject json,
        IEnumerable<string> guildIds,
        IEnumerable<string> channelIds,
        IEnumerable<string> userIds
    )
    {
        var guilds = new List<Guild>();
        foreach (var chunk in guildIds.Distinct().Chunk(ChunkSize))
            guilds.AddRange(await repository.Guild.GetGuildsByIdsAsync([.. chunk]));

        var channels = new List<GuildChannel>();
        foreach (var chunk in channelIds.Distinct().Chunk(ChunkSize))
            channels.AddRange(await repository.Channel.GetChannelsByIdsAsync([.. chunk]));

        var users = new List<Database.Entity.User>();
        foreach (var chunk in userIds.Distinct().Chunk(ChunkSize))
            users.AddRange(await repository.User.GetUsersByIdsAsync([.. chunk]));

        json["Guilds"] = TransformGuilds(guilds);
        json["Channels"] = TransformChannels(channels);
        json["Users"] = TransformUsers(users);
    }

    private static JsonArray TransformGuilds(IEnumerable<Guild?> guilds)
    {
        var items = guilds
            .Where(o => o is not null)
            .DistinctBy(o => o!.Id)
            .Select(o => (JsonNode)new JsonObject
            {
                ["Id"] = o!.Id,
                ["Name"] = o.Name
            });

        return [with([.. items])];
    }

    private static JsonArray TransformUsers(IEnumerable<User?> users)
    {
        var items = users
            .Where(o => o is not null)
            .DistinctBy(o => o!.Id)
            .Select(o => (JsonNode)TransformUser(o!));

        return [with([.. items])];
    }

    private static JsonObject TransformUser(User user)
    {
        var json = new JsonObject
        {
            ["Id"] = user.Id,
            ["FullName"] = user.Username
        };

        if (user.Flags > 0)
            json["Flags"] = user.Flags;

        return json;
    }

    private static JsonArray TransformChannels(IEnumerable<GuildChannel?> channels)
    {
        var items = channels
            .Where(o => o is not null)
            .DistinctBy(o => $"{o!.ChannelId}/{o.GuildId}")
            .Select(ch =>
            {
                var channel = new JsonObject
                {
                    ["Id"] = ch!.ChannelId,
                    ["Name"] = ch.Name,
                    ["Type"] = ch.ChannelType.ToString(),
                    ["GuildId"] = ch.GuildId
                };

                if (ch.UserPermissionsCount > 0)
                    channel["UserPermissionsCount"] = ch.UserPermissionsCount;
                if (ch.RolePermissionsCount > 0)
                    channel["RolePermissionsCount"] = ch.RolePermissionsCount;
                if (ch.Flags > 0)
                    channel["Flags"] = ch.Flags;
                if (!string.IsNullOrEmpty(ch.ParentChannelId))
                    channel["ParentChannelId"] = ch.ParentChannelId;

                return (JsonNode)channel;
            });

        return [with([.. items])];
    }
}
