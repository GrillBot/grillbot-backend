namespace GrillBot.Contracts.AuditLog.Events.Create;

public class InteractionCommandParameterRequest
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;

    public InteractionCommandParameterRequest()
    {
    }

    public InteractionCommandParameterRequest(string name, string type, string value)
    {
        Name = name;
        Type = type;
        Value = value;
    }
}
