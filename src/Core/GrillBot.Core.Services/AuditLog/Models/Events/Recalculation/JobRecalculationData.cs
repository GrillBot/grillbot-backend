namespace AuditLog.Models.Events.Recalculation;

public class JobRecalculationData
{
    public DateOnly JobDate { get; set; }
    public string JobName { get; set; } = null!;

    public JobRecalculationData()
    {
    }

    public JobRecalculationData(DateOnly jobDate, string jobName)
    {
        JobDate = jobDate;
        JobName = jobName;
    }

    public override string ToString()
        => $"{JobDate:yyyyMMdd} {JobName}";
}
