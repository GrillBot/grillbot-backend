using System.Net;

namespace GrillBot.Core.Services.IdentityService;

public class IdentityTokenException(HttpStatusCode statusCode, string? error, string? errorDescription)
    : Exception($"IdentityService refused the token request ({(int)statusCode}): {error ?? "unknown_error"} {errorDescription}".TrimEnd())
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}
