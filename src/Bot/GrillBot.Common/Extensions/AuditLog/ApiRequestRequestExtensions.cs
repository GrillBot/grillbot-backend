using GrillBot.Core.Infrastructure;
using GrillBot.Contracts.AuditLog.Events.Create;
using Microsoft.Extensions.Primitives;

namespace GrillBot.Common.Extensions.AuditLog;

public static class ApiRequestRequestExtensions
{
    public static void AddParameter(this ApiRequestRequest request, string? name, string? value)
    {
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            request.Parameters.Add(name, value);
    }

    public static void AddHeaders(this ApiRequestRequest request, string? name, StringValues values)
    {
        if (string.IsNullOrEmpty(name))
            return;

        // A header sent more than once arrives as multiple values; combine them as HTTP does (RFC 9110 §5.3).
        var value = string.Join(", ", values.Where(o => !string.IsNullOrEmpty(o)));
        if (!string.IsNullOrEmpty(value))
            request.Headers[name] = value;
    }

    public static void AddParameters(this ApiRequestRequest request, IDictionaryObject? apiObject, int index = -1)
    {
        if (apiObject is null)
            return;

        foreach (var item in apiObject.ToDictionary())
            request.AddParameter(item.Key + (index > -1 ? $"[{index}]" : ""), item.Value);
    }
}
