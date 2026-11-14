using GrillBot.Common.Extensions;
using GrillBot.Contracts.AuditLog.Enums;
using GrillBot.Contracts.AuditLog.Events.Create;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace GrillBot.App.Managers.Auth;

/// <summary>
/// Writes identity / access events (sign-in, refused sign-in, sign-out, revoked session) to
/// AuditLogService. Never records a token, cookie or session ID - only who, where, why and from
/// which IP address.
/// </summary>
public class IdentityAuditLog(
    IMessageBus _bus,
    IHttpContextAccessor _httpContextAccessor,
    ILogger<IdentityAuditLog> _logger
)
{
    public async Task WriteAsync(IdentityEventType eventType, ulong? userId, string channel, string? reason = null)
    {
        try
        {
            var ipAddress = _httpContextAccessor.HttpContext?.GetRemoteIp();
            var request = new LogRequest(LogType.Identity, DateTime.UtcNow, userId: userId?.ToString())
            {
                IdentityEvent = new IdentityEventRequest(eventType, channel, string.IsNullOrWhiteSpace(reason) ? null : reason, ipAddress)
            };

            await _bus.PublishAsync(new CreateItemsMessage(request));
        }
        catch (Exception ex)
        {
            // Auditing must never decide whether someone can sign in or out.
            _logger.LogWarning(ex, "Could not write the identity audit event {EventType} for user {UserId}.", eventType, userId);
        }
    }
}
