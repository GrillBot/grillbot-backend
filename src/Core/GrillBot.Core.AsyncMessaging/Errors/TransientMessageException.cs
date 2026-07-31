namespace GrillBot.Core.AsyncMessaging.Errors;

/// <summary>
/// Thrown by a handler when the message is fine but the world is not yet ready for it -
/// a missing row that is still being written, a Discord entity not visible yet. It is the
/// replacement for the old RabbitConsumptionResult.Retry and is the one exception type
/// that gets its own retry policy before the generic one takes over.
/// </summary>
public class TransientMessageException : Exception
{
    public TransientMessageException(string message) : base(message)
    {
    }

    public TransientMessageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
