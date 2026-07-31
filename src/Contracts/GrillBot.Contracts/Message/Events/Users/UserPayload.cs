using Discord;

namespace GrillBot.Contracts.Message.Events.Users;

public sealed record UserPayload(ulong Id, DateTimeOffset CreatedAt, bool IsBot, bool IsWebhook)
{
    public bool IsUser()
        => !IsBot && !IsWebhook;

    public static UserPayload Create(IUser user)
        => new(user.Id, user.CreatedAt, user.IsBot, user.IsWebhook);
}
