using GrillBot.Core.Database.ValueObjects;

namespace UserManagementService.Core.Entity.Infractions;

public abstract class InfractionBase : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public DiscordIdValueObject ModeratorId { get; set; }
    public DiscordIdValueObject GuildId { get; set; }
    public DiscordIdValueObject UserId { get; set; }
    public string Reason { get; set; } = null!;
}
