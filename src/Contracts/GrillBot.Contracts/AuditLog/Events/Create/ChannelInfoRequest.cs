namespace GrillBot.Contracts.AuditLog.Events.Create;

public class ChannelInfoRequest
{
    public string? Topic { get; set; }
    public int Position { get; set; }
    public int Flags { get; set; }

    public ChannelInfoRequest()
    {
    }

    public ChannelInfoRequest(string? topic, int position, int flags)
    {
        Topic = topic;
        Position = position;
        Flags = flags;
    }
}
