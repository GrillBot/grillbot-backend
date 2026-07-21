using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using ImageProcessing.Models;
using Refit;

namespace ImageProcessing;

[Service("ImageProcessing")]
public interface IImageProcessingClient : IServiceClient
{
    [Post("/api/image/peepoLove")]
    Task<Stream> CreatePeepoloveImageAsync(PeepoRequest request, CancellationToken cancellationToken = default);

    [Post("/api/image/peepoangry")]
    Task<Stream> CreatePeepoangryImageAsync(PeepoRequest request, CancellationToken cancellationToken = default);

    [Post("/api/image/points")]
    Task<Stream> CreatePointsImageAsync(PointsRequest request, CancellationToken cancellationToken = default);

    [Post("/api/image/without-accident")]
    Task<Stream> CreateWithoutAccidentImageAsync(WithoutAccidentImageRequest request, CancellationToken cancellationToken = default);

    [Post("/api/image/chart")]
    Task<Stream> CreateChartImageAsync(ChartRequest request, CancellationToken cancellationToken = default);
}
