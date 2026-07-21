using GrillBot.Core.Managers.Performance;

namespace GrillBot.Core.Services.Common.Handlers;

public class HttpClientHandler(ICounterManager _counterManager) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var serviceName = GetServiceName(request);

        using (_counterManager.Create($"Service.{serviceName}"))
            return await base.SendAsync(request, cancellationToken);
    }

    private static string GetServiceName(HttpRequestMessage request)
    {
        return request.Options.TryGetValue(new("ServiceName"), out string? serviceName) && !string.IsNullOrEmpty(serviceName)
            ? serviceName
            : $"({request.RequestUri})";
    }
}
