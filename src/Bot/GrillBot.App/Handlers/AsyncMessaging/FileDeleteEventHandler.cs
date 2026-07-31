using GrillBot.App.Helpers;
using GrillBot.Common.FileStorage;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Contracts.AuditLog.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.AsyncMessaging;

public class FileDeleteEventHandler(BlobManagerFactoryHelper _blobManagerFactory)
{
    public async Task HandleAsync(FileDeletePayload message, CancellationToken cancellationToken)
    {
        var blobManager = await _blobManagerFactory.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
        var legacyManager = await _blobManagerFactory.CreateLegacyAsync();

        await legacyManager.DeleteAsync(message.Filename);
        await blobManager.DeleteAsync(message.Filename);
    }
}
