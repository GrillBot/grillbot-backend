namespace GrillBot.Contracts.AuditLog.Events.Create;

public class FileRequest
{
    public string Filename { get; set; } = null!;
    public string? Extension { get; set; }
    public long Size { get; set; }

    public FileRequest()
    {
    }

    public FileRequest(string filename, string? extension, long size)
    {
        Filename = filename;
        Extension = extension;
        Size = size;
    }
}
