using AuditLogService.Core.Entity;
using AuditLogService.Processors.Request.Abstractions;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.Extensions;

namespace AuditLogService.Processors.Request;

public class IdentityEventProcessor(IServiceProvider serviceProvider) : RequestProcessorBase(serviceProvider)
{
    public override Task ProcessAsync(LogItem entity, LogRequest request)
    {
        var identityEvent = request.IdentityEvent;
        if (identityEvent is null)
        {
            entity.CanCreate = false;
            return Task.CompletedTask;
        }

        entity.IdentityEvent = new IdentityEvent
        {
            EventType = identityEvent.EventType,
            Channel = identityEvent.Channel.Cut(32, true)!,
            Reason = identityEvent.Reason.Cut(1024),
            IpAddress = identityEvent.IpAddress.Cut(64, true)
        };

        return Task.CompletedTask;
    }
}
