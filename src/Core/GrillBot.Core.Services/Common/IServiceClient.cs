using GrillBot.Core.Services.Diagnostics.Models;
using Refit;

namespace GrillBot.Core.Services.Common;

public interface IServiceClient
{
    [Head("/health")]
    [Headers("User-Agent: GrillBot")]
    Task IsHealthyAsync(CancellationToken cancellationToken = default);

    [Get("/api/diag")]
    Task<DiagnosticInfo> GetDiagnosticsAsync(CancellationToken cancellationToken = default);

    [Get("/api/diag/uptime")]
    Task<long> GetUptimeAsync(CancellationToken cancellationToken = default);
}
