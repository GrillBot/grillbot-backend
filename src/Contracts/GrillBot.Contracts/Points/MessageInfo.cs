using Discord;

namespace GrillBot.Contracts.Points;

public class MessageInfo
{
    public string Id { get; set; } = null!;
    public int ContentLength { get; set; }
    public MessageType MessageType { get; set; }
    public string AuthorId { get; set; } = null!;
}
