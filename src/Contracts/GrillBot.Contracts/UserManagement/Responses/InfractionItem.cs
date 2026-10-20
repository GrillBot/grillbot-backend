namespace GrillBot.Contracts.UserManagement.Responses;

public class InfractionItem
{
    public Guid InfractionId { get; set; }
    public string Type { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public string ModeratorId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string GuildId { get; set; } = null!;
    public DateTime? ValidTo { get; set; }
    public string Reason { get; set; } = null!;
}
