namespace AuditLog.Models.Events.Create;

public class LogMessageRequest
{
    public string Message { get; set; } = null!;
    public string SourceAppName { get; set; } = null!;
    public string Source { get; set; } = null!;

    public LogMessageRequest()
    {
    }

    public LogMessageRequest(string message, string sourceAppName, string source)
    {
        Message = message;
        SourceAppName = sourceAppName;
        Source = source;
    }
}
