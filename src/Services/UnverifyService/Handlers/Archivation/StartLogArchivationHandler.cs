using GrillBot.Contracts.Unverify.Events;
using GrillBot.Core.IO.SharedStorage;
using GrillBot.Services.Common.Infrastructure.AsyncMessaging;
using Microsoft.Extensions.Options;
using UnverifyService.Core.Archivation;
using UnverifyService.Core.Entity;
using UnverifyService.Options;

namespace UnverifyService.Handlers.Archivation;

/// <summary>
/// Builds one archive of expired unverify log records into the shared storage and publishes its file name as
/// <see cref="UnverifyLogArchiveDataPreparedEvent"/>. Records are streamed page by page into a compressed JSON
/// file, so memory use is bounded by <see cref="ArchivationOptions.ReadPageSize"/>, not by the size of the archive.
/// </summary>
public partial class StartLogArchivationHandler(
    IServiceProvider serviceProvider,
    UnverifyLogRetention _retention,
    SharedFileStorage _sharedStorage,
    IOptions<AppOptions> _options
) : EventHandlerBaseWithDb<UnverifyContext>(serviceProvider)
{
    /// <summary>Prefix of the archive files in the shared storage.</summary>
    public const string SharedFilePrefix = "Unverify";

    private ArchivationOptions Options => _options.Value.Archivation;

    public async Task HandleAsync(StartUnverifyLogArchivationCommand command, CancellationToken cancellationToken)
    {
        DeleteLeftoverFiles();

        var expiredQuery = _retention.WhereExpired(DbContext.LogItems);

        var count = await ContextHelper.ReadCountAsync(expiredQuery, cancellationToken);
        if (count == 0 || count < Options.MinimalCount)
        {
            if (Logger.IsEnabled(LogLevel.Information))
                Logger.LogInformation("Unverify log archivation skipped. Expired records: {Count}, minimum: {Minimum}.", count, Options.MinimalCount);
            return;
        }

        // An uncommitted file (failure, or nothing written) is deleted when the writer is disposed.
        await using var file = _sharedStorage.Create(SharedFilePrefix, ".json.br");

        var archive = await CreateArchiveAsync(ReadItemsToArchiveAsync(expiredQuery, cancellationToken), file.Stream, cancellationToken);
        if (archive.ItemsCount == 0)
            return;

        await file.CommitAsync(cancellationToken);
        archive = archive with
        {
            ContentFileName = file.Name,
            Sequence = command.Sequence,
            HasMore = count > archive.ItemsCount
        };

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "Unverify log archive {FileName} prepared (run {Sequence}). Records: {Count}, JSON: {JsonSize} B, compressed: {CompressedSize} B, more pending: {HasMore}.",
                archive.ContentFileName, archive.Sequence, archive.ItemsCount, archive.JsonSize, archive.CompressedSize, archive.HasMore
            );
        }

        await Publisher.PublishAsync(archive);
    }

    /// <summary>Files of crashed runs and of events that were never consumed (dead-lettered).</summary>
    private void DeleteLeftoverFiles()
    {
        var deleted = _sharedStorage.DeleteExpired(SharedFilePrefix, Options.SharedFileRetention);
        if (deleted > 0)
            Logger.LogWarning("Deleted {Count} unverify log archive files older than {Retention} from the shared storage. Their archives were never stored.", deleted, Options.SharedFileRetention);
    }
}
