namespace GrillBot.Contracts.Emote.Responses;

public class EmoteUserUsageItem
{
    public string UserId { get; set; } = null!;
    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }
}
