namespace PointsService.Models;

public class ReactionInfo
{
    public string UserId { get; set; } = null!;
    public string Emote { get; set; } = null!;
    public bool IsBurst { get; set; }

    public string GetReactionId()
    {
        var id = $"{UserId}_{Emote}";
        return IsBurst ? id + "_Burst" : id;
    }
}
