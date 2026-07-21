using System.Net;

namespace GrillBot.Core.Services.Common.Exceptions;

public class ClientNotAcceptableException : ClientException
{
    public ClientNotAcceptableException() : base(HttpStatusCode.NotAcceptable)
    {
    }

    public ClientNotAcceptableException(string? message) : base(message)
    {
    }

    public ClientNotAcceptableException(HttpStatusCode statusCode) : base(statusCode)
    {
    }

    public ClientNotAcceptableException(string? message, Exception? inner) : base(message, inner)
    {
    }

    public ClientNotAcceptableException(HttpStatusCode statusCode, string content) : base(statusCode, content)
    {
    }

    public ClientNotAcceptableException(string? message, Exception? inner, HttpStatusCode? statusCode) : base(message, inner, statusCode)
    {
    }

    public ClientNotAcceptableException(HttpRequestError httpRequestError, string? message = null, Exception? inner = null, HttpStatusCode? statusCode = null) : base(httpRequestError, message, inner, statusCode)
    {
    }
}
