using GrillBot.Contracts.Searching.Events.Users;

namespace GrillBot.Contracts.Searching.Events;

public sealed record SynchronizationPayload(List<UserSynchronizationItem> Users);
