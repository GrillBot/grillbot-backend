namespace AuditLog.Models.Events.Create;

public class InteractionCommandRequest
{
    public string Name { get; set; } = null!;
    public string ModuleName { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public List<InteractionCommandParameterRequest> Parameters { get; set; } = [];
    public bool HasResponded { get; set; }
    public bool IsValidToken { get; set; }
    public bool IsSuccess { get; set; }
    public int? CommandError { get; set; }
    public string? ErrorReason { get; set; }
    public int Duration { get; set; }
    public string? Exception { get; set; }
    public string Locale { get; set; } = "cs";

    public InteractionCommandRequest()
    {
    }

    public InteractionCommandRequest(
        string name,
        string moduleName,
        string methodName,
        List<InteractionCommandParameterRequest> parameters,
        bool hasResponded,
        bool isValidToken,
        bool isSuccess,
        int? commandError,
        string? errorReason,
        int duration,
        string? exception,
        string locale
    )
    {
        Name = name;
        ModuleName = moduleName;
        MethodName = methodName;
        Parameters = parameters;
        HasResponded = hasResponded;
        IsValidToken = isValidToken;
        IsSuccess = isSuccess;
        CommandError = commandError;
        ErrorReason = errorReason;
        Duration = duration;
        Exception = exception;
        Locale = locale;
    }
}
