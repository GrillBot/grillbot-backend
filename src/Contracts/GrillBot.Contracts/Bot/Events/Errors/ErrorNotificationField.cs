namespace GrillBot.Contracts.Bot.Events.Errors;

public class ErrorNotificationField
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public bool IsInline { get; set; }

    public ErrorNotificationField()
    {
    }

    public ErrorNotificationField(string key, string value, bool isInline)
    {
        Key = key;
        Value = value;
        IsInline = isInline;
    }
}
