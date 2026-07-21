namespace GrillBot.Core.Exceptions;

public class GrillBotException : Exception
{
    public GrillBotException()
    {
    }

    public GrillBotException(string message) : base(message)
    {
    }

    public GrillBotException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
