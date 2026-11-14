using GrillBot.Contracts.AuditLog.Enums;

namespace GrillBot.Contracts.AuditLog.Events.Create;

/// <summary>
/// An identity / access event. The user it concerns goes into <see cref="LogRequest.UserId"/>;
/// no token, cookie or session ID ever goes in here.
/// </summary>
public class IdentityEventRequest
{
    public IdentityEventType EventType { get; set; }

    /// <summary>Where it happened - one of <c>IdentityProtocol.Channels</c> (web, legacy, ...).</summary>
    public string Channel { get; set; } = null!;

    /// <summary>Why a sign-in was refused or a session revoked. Empty for successful events.</summary>
    public string? Reason { get; set; }

    /// <summary>The client's IP address as the gateway saw it (first X-Forwarded-For hop).</summary>
    public string? IpAddress { get; set; }

    public IdentityEventRequest()
    {
    }

    public IdentityEventRequest(IdentityEventType eventType, string channel, string? reason, string? ipAddress)
    {
        EventType = eventType;
        Channel = channel;
        Reason = reason;
        IpAddress = ipAddress;
    }
}
