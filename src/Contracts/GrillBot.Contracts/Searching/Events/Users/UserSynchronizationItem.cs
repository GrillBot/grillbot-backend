using Discord;

namespace GrillBot.Contracts.Searching.Events.Users;

public sealed record UserSynchronizationItem(
    string GuildId,
    string UserId,
    bool IsAdmin,
    GuildPermission GuildPermissions
);
