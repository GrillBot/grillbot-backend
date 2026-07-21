using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.Services.Common.Attributes;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace GrillBot.Core.Services.Common.Executor;

public class ServiceClientExecutor<TServiceInterface>(
    IConfiguration _configuration,
    TServiceInterface _serviceClient,
    ICurrentUserProvider _currentUser
) : IServiceClientExecutor<TServiceInterface> where TServiceInterface : IServiceClient
{
    public async Task<TResult> ExecuteRequestAsync<TResult>(Func<TServiceInterface, ServiceExecutorContext, Task<TResult>> executeRequest, CancellationToken cancellationToken = default)
    {
        using var timeoutCancellationTokenSource = CreateTimeoutCancellationToken();
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token, cancellationToken);

        var context = CreateContext(cancellationTokenSource);
        return await executeRequest(_serviceClient, context);
    }

    public async Task ExecuteRequestAsync(Func<TServiceInterface, ServiceExecutorContext, Task> executionRequest, CancellationToken cancellationToken = default)
    {
        using var timeoutCancellationTokenSource = CreateTimeoutCancellationToken();
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token, cancellationToken);

        var context = CreateContext(cancellationTokenSource);
        await executionRequest(_serviceClient, context);
    }

    private CancellationTokenSource CreateTimeoutCancellationToken()
    {
        var timeout = GetTimeout();
        return new CancellationTokenSource(timeout);
    }

    private TimeSpan GetTimeout()
    {
        var serviceName = typeof(TServiceInterface).GetCustomAttribute<ServiceAttribute>()!.ServiceName;
        return _configuration.GetValue<TimeSpan>($"Services:{serviceName}:Timeout");
    }

    private ServiceExecutorContext CreateContext(CancellationTokenSource cancellationTokenSource)
    {
        return new ServiceExecutorContext(_currentUser.EncodedJwtToken, cancellationTokenSource.Token);
    }
}
