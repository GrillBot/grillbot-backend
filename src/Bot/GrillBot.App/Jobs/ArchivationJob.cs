using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Contracts.AuditLog.Events;
using GrillBot.Contracts.Unverify.Events;
using Quartz;
using Wolverine;

namespace GrillBot.App.Jobs;

/// <summary>
/// Starts archivation of the audit log and the unverify log. The services decide whether anything expired
/// and do the work asynchronously; each finished run is recorded as an Archivation audit log record,
/// so the job itself reports nothing.
/// </summary>
[DisallowConcurrentExecution]
public class ArchivationJob(IServiceProvider serviceProvider) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var messageBus = ResolveService<IMessageBus>();

        await messageBus.PublishAsync(new StartAuditLogArchivationCommand());
        await messageBus.PublishAsync(new StartUnverifyLogArchivationCommand());
    }
}
