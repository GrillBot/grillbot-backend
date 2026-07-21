namespace AuditLog.Models.Events.Recalculation;

public class InteractionRecalculationData
{
    public string Name { get; set; } = null!;
    public string ModuleName { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public bool IsSuccess { get; set; }
    public DateOnly EndDate { get; set; }
    public string UserId { get; set; } = null!;

    public InteractionRecalculationData()
    {
    }

    public InteractionRecalculationData(string name, string moduleName, string methodName, bool isSuccess, DateOnly endDate, string userId)
    {
        Name = name;
        ModuleName = moduleName;
        MethodName = methodName;
        IsSuccess = isSuccess;
        EndDate = endDate;
        UserId = userId;
    }

    public override string ToString()
        => $"{Name} ({ModuleName} / {MethodName}) {IsSuccess} {EndDate:yyyyMMdd} {UserId}";
}
