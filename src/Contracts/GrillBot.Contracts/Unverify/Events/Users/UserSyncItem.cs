namespace GrillBot.Contracts.Unverify.Events.Users;

/// <summary>
/// One user inside a <see cref="SynchronizationMessage"/>. Not a message of its own.
/// </summary>
public sealed record UserSyncItem(ulong UserId, bool IsBot, string? UserLanguage);
