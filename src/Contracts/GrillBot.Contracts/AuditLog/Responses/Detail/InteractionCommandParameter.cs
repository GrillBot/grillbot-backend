namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class InteractionCommandParameter
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
}
