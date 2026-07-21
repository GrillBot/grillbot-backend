namespace AuditLog.Models.Events.Create;

public class JobExecutionRequest
{
    public string JobName { get; set; } = null!;
    public string Result { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool WasError { get; set; }
    public string? StartUserId { get; set; }

    public JobExecutionRequest()
    {
    }

    public JobExecutionRequest(string jobName, string result, DateTime startAt, DateTime endAt, bool wasError, string? startUserId)
    {
        JobName = jobName;
        Result = result;
        StartAt = startAt;
        EndAt = endAt;
        WasError = wasError;
        StartUserId = startUserId;
    }
}
