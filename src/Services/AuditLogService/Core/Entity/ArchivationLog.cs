using System.ComponentModel.DataAnnotations;
using GrillBot.Contracts.AuditLog.Enums;

namespace AuditLogService.Core.Entity;

/// <summary>Report of one finished archivation run (<see cref="LogType.Archivation"/>).</summary>
public class ArchivationLog : ChildEntityBase
{
    /// <summary>One of <c>ArchivationProcessNames</c>: AuditLog, Unverify.</summary>
    [StringLength(32)]
    public string ProcessName { get; set; } = null!;

    public int TotalRecordsCount { get; set; }

    /// <summary>Uncompressed, serialized size of the final JSON in bytes.</summary>
    public long JsonSize { get; set; }

    /// <summary>Compressed size of the final archive in bytes.</summary>
    public long ArchiveSize { get; set; }

    public string? Details { get; set; }

    /// <summary>Null when the process does not delete files.</summary>
    public int? DeletedFilesCount { get; set; }

    /// <summary>Null when the process does not delete files.</summary>
    public long? DeletedFilesSize { get; set; }
}
