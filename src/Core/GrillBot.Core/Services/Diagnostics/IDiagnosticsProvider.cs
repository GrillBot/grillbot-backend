using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Core.Services.Diagnostics;

public interface IDiagnosticsProvider
{
    Task<DiagnosticInfo> GetInfoAsync(CancellationToken cancellationToken = default);
}
