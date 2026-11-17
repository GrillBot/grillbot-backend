using AuditLogService.Core.Entity;
using AuditLogService.Processors.Request.Abstractions;
using GrillBot.Contracts.AuditLog.Events.Create;
using GrillBot.Core.Extensions;

namespace AuditLogService.Processors.Request;

public class ArchivationProcessor(IServiceProvider serviceProvider) : RequestProcessorBase(serviceProvider)
{
    public override Task ProcessAsync(LogItem entity, LogRequest request)
    {
        var archivation = request.Archivation;

        // An archivation run that archived nothing is not worth a record.
        if (archivation is null || archivation.TotalRecordsCount <= 0 || string.IsNullOrEmpty(archivation.ProcessName))
        {
            entity.CanCreate = false;
            return Task.CompletedTask;
        }

        entity.Archivation = new ArchivationLog
        {
            ProcessName = archivation.ProcessName.Cut(32, true)!,
            TotalRecordsCount = archivation.TotalRecordsCount,
            JsonSize = Math.Max(archivation.JsonSize, 0),
            ArchiveSize = Math.Max(archivation.ArchiveSize, 0),
            Details = archivation.Details,
            DeletedFilesCount = archivation.DeletedFilesCount,
            DeletedFilesSize = archivation.DeletedFilesSize
        };

        return Task.CompletedTask;
    }
}
