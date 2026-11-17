using GrillBot.Services.Common.Telemetry;
using Microsoft.EntityFrameworkCore;
using UnverifyService.Core.Archivation;
using UnverifyService.Core.Entity;

namespace UnverifyService.Telemetry.Initializers;

public class ArchivationInitializer(
    IServiceProvider serviceProvider,
    UnverifyTelemetryCollector _collector
) : TelemetryInitializerBase(serviceProvider)
{
    protected override async Task ExecuteInternalAsync(IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var retention = provider.GetRequiredService<UnverifyLogRetention>();
        var dbContext = provider.GetRequiredService<UnverifyContext>();

        _collector.ItemsToArchive.Set(await retention.WhereExpired(dbContext.LogItems.AsNoTracking()).CountAsync(cancellationToken));
    }
}
