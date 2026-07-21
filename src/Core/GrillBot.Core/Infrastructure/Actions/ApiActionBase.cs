using GrillBot.Core.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;

namespace GrillBot.Core.Infrastructure.Actions;

public abstract class ApiActionBase : IDisposable
{
    private CancellationToken? _manualCancellationToken = null;
    private CancellationTokenSource? _cancellationTokenSource = null;
    private bool disposedValue;

    protected HttpContext HttpContext { get; private set; } = null!;
    protected object?[] Parameters { get; set; } = null!;
    protected ICurrentUserProvider CurrentUser { get; private set; } = null!;

    protected CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

    public void Init(HttpContext httpContext, object?[] parameters, ICurrentUserProvider currentUser)
    {
        Parameters = parameters;
        HttpContext = httpContext;
        CurrentUser = currentUser;

        ConfigureCancellationTokens();
    }

    public abstract Task<ApiResult> ProcessAsync();

    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        _manualCancellationToken = cancellationToken;
        ConfigureCancellationTokens();
    }

    protected T GetParameter<T>(int index)
        => (T)Parameters[index]!;

    protected T? GetOptionalParameter<T>(int index)
    {
        var item = Parameters.ElementAtOrDefault(index);
        return item is null ? default : (T)item;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void ConfigureCancellationTokens()
    {
        _cancellationTokenSource?.Dispose();

        var tokens = new[] { _manualCancellationToken, HttpContext?.RequestAborted }.Where(o => o is not null).Select(o => o!.Value).ToArray();
        _cancellationTokenSource = tokens.Length == 0 ? null : CancellationTokenSource.CreateLinkedTokenSource(tokens);
    }
}
