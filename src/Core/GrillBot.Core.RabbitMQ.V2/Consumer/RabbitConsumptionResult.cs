namespace GrillBot.Core.RabbitMQ.V2.Consumer;

public enum RabbitConsumptionResult
{
    Success,
    Retry,
    Reject
}
