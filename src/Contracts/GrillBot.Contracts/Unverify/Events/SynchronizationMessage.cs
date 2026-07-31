using GrillBot.Contracts.Unverify.Events.Users;

namespace GrillBot.Contracts.Unverify.Events;

public sealed record SynchronizationMessage(List<UserSyncItem> Users);
