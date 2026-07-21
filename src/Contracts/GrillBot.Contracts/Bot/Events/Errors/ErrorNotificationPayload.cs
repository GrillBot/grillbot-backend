using GrillBot.Core.RabbitMQ.V2.Messages;

namespace GrillBot.Contracts.Bot.Events.Errors;

public class ErrorNotificationPayload : IRabbitMessage
{
    public string Topic => "GrillBot";
    public string Queue => "ErrorNotification";

    public string? Title { get; set; }
    public List<ErrorNotificationField> Fields { get; set; } = [];
    public ulong? UserId { get; set; }

    public ErrorNotificationPayload()
    {
    }

    public ErrorNotificationPayload(string? title, IEnumerable<ErrorNotificationField> fields, ulong? userId)
    {
        Title = title;
        Fields = [.. fields.Where(o => o is not null)];
        UserId = userId;
    }
}
