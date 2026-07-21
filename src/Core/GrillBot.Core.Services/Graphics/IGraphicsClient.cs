using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using Graphics.Models.Chart;
using Refit;

namespace Graphics;

[Service("Graphics")]
public interface IGraphicsClient : IServiceClient
{
    [Post("/chart")]
    Task<Stream> CreateChartAsync(ChartRequestData request, CancellationToken cancellationToken = default);
}
