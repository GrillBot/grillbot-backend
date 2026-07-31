using System.Text.Json.Serialization;
using Discord;

namespace GrillBot.Contracts.Invite.Events;

[method: JsonConstructor]
public sealed record InviteCreatedPayload(
    string Code,
    string GuildId,
    int Uses,
    string? CreatorId,
    DateTime? CreatedAt
)
{
    public InviteCreatedPayload(IInviteMetadata metadata) : this(
        metadata.Code,
        metadata.GuildId.ToString()!,
        metadata.Uses ?? 0,
        metadata.Inviter?.Id.ToString(),
        metadata.CreatedAt?.UtcDateTime
    )
    {
    }
}
