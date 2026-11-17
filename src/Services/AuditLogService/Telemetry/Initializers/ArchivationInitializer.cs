using AuditLogService.Core.Archivation;
using AuditLogService.Core.Entity;
using Microsoft.EntityFrameworkCore;
using GrillBot.Services.Common.Telemetry;

namespace AuditLogService.Telemetry.Initializers;

public class ArchivationInitializer(
    IServiceProvider serviceProvider,
    AuditLogTelemetryCollector _collector
) : TelemetryInitializerBase(serviceProvider)
{
    protected override async Task ExecuteInternalAsync(IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var retention = provider.GetRequiredService<AuditLogRetention>();
        var dbContext = provider.GetRequiredService<AuditLogServiceContext>();

        _collector.ItemsOfArchive.Set(await retention.WhereExpired(dbContext.LogItems.AsNoTracking()).CountAsync(cancellationToken));
    }
}
