using GrillBot.Contracts.Points.Channels;
using GrillBot.Contracts.Points.Users;

namespace GrillBot.Contracts.Points.Events;

public sealed record SynchronizationPayload(
    string GuildId,
    List<ChannelSyncItem> Channels,
    List<UserSyncItem> Users
);
