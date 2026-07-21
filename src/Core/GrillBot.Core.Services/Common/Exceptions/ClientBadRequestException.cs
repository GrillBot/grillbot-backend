using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GrillBot.Core.Services.Common.Exceptions;

public class ClientBadRequestException : ClientException
{
    public Dictionary<string, string[]> ValidationErrors { get; } = [];
    public string? RawData { get; }

    public ClientBadRequestException()
    {
    }

    public ClientBadRequestException(string? message) : base(message)
    {
    }

    public ClientBadRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ClientBadRequestException(ValidationProblemDetails problemDetails, string? rawData) : base(HttpStatusCode.BadRequest)
    {
        ValidationErrors = problemDetails.Errors.ToDictionary(o => o.Key, o => o.Value);
        RawData = rawData;
    }

    public ClientBadRequestException(string? message, Exception? inner, HttpStatusCode? statusCode) : base(message, inner, statusCode)
    {
    }

    public ClientBadRequestException(HttpStatusCode statusCode) : base(statusCode)
    {
    }

    public ClientBadRequestException(HttpStatusCode statusCode, string content) : base(statusCode, content)
    {
    }
}
