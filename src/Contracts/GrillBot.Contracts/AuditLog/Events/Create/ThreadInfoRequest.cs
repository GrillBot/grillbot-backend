using Discord;

namespace GrillBot.Contracts.AuditLog.Events.Create;

public class ThreadInfoRequest
{
    public string? ThreadName { get; set; }
    public int? SlowMode { get; set; }
    public ThreadType Type { get; set; }
    public bool IsArchived { get; set; }
    public int ArchiveDuration { get; set; }
    public bool IsLocked { get; set; }
    public List<string> Tags { get; set; } = [];

    public ThreadInfoRequest()
    {
    }

    public ThreadInfoRequest(string? threadName, int? slowMode, ThreadType type, bool isArchived, int archiveDuration, bool isLocked, List<string> tags)
    {
        ThreadName = threadName;
        SlowMode = slowMode;
        Type = type;
        IsArchived = isArchived;
        ArchiveDuration = archiveDuration;
        IsLocked = isLocked;
        Tags = tags;
    }
}
