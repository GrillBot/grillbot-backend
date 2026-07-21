namespace GrillBot.Contracts.AuditLog.Responses.Detail;

public class EmbedField
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public bool Inline { get; set; }
}
