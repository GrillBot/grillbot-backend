using System.Net;

namespace GrillBot.Core.Services.Common.Exceptions;

public class ClientNotFoundException : ClientException
{
    public ClientNotFoundException() : base(HttpStatusCode.NotFound)
    {
    }

    public ClientNotFoundException(string? message) : base(message)
    {
    }

    public ClientNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ClientNotFoundException(HttpStatusCode statusCode, string content) : base(statusCode, content)
    {
    }

    public ClientNotFoundException(string? message, Exception? inner, HttpStatusCode? statusCode) : base(message, inner, statusCode)
    {
    }

    public ClientNotFoundException(HttpStatusCode statusCode) : base(statusCode)
    {
    }

    public ClientNotFoundException(HttpRequestError httpRequestError, string? message = null, Exception? inner = null, HttpStatusCode? statusCode = null) : base(httpRequestError, message, inner, statusCode)
    {
    }
}
