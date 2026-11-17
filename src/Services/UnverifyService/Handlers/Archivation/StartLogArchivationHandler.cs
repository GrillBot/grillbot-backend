using GrillBot.Contracts.Unverify.Events;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Options;
using UnverifyService.Core.Archivation;
using UnverifyService.Core.Entity;
using UnverifyService.Options;

namespace UnverifyService.Handlers.Archivation;

/// <summary>
/// Builds one archive of expired unverify log records and publishes it as <see cref="UnverifyLogArchiveDataPreparedEvent"/>.
/// Records are streamed page by page into a compressed JSON writer, so memory use is bounded by
/// <see cref="ArchivationOptions.ReadPageSize"/>, not by the size of the archive.
/// </summary>
public partial class StartLogArchivationHandler(
    IServiceProvider serviceProvider,
    UnverifyLogRetention _retention,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    private ArchivationOptions Options => _options.Value.Archivation;

    public async Task HandleAsync(StartUnverifyLogArchivationCommand _, CancellationToken cancellationToken)
    {
        var expiredQuery = _retention.WhereExpired(DbContext.LogItems);

        var count = await ContextHelper.ReadCountAsync(expiredQuery, cancellationToken);
        if (count == 0 || count < Options.MinimalCount)
        {
            if (Logger.IsEnabled(LogLevel.Information))
                Logger.LogInformation("Unverify log archivation skipped. Expired records: {Count}, minimum: {Minimum}.", count, Options.MinimalCount);
            return;
        }

        var archive = await CreateArchiveAsync(ReadItemsToArchiveAsync(expiredQuery, cancellationToken), cancellationToken);
        if (archive.ItemsCount == 0)
            return;

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "Unverify log archive prepared. Records: {Count}, JSON: {JsonSize} B, compressed: {CompressedSize} B.",
                archive.ItemsCount, archive.JsonSize, archive.CompressedContent.LongLength
            );
        }

        await Publisher.PublishAsync(archive);
    }
}
