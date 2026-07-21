using GrillBot.Core.RabbitMQ.V2.Messages;
using UnverifyService.Models.Request;

namespace UnverifyService.Models.Events;

public class SetUnverifyMessage : UnverifyRequest, IRabbitMessage
{
    public string Topic => "Unverify";
    public string Queue => "SetUnverify";

    public SetUnverifyMessage()
    {
    }

    public SetUnverifyMessage(UnverifyRequest request)
    {
        GuildId = request.GuildId;
        UserId = request.UserId;
        ChannelId = request.ChannelId;
        MessageId = request.MessageId;
        EndAtUtc = request.EndAtUtc;
        Reason = request.Reason;
        TestRun = request.TestRun;
        IsSelfUnverify = request.IsSelfUnverify;
        RequiredKeepables = request.RequiredKeepables ?? [];
    }
}
