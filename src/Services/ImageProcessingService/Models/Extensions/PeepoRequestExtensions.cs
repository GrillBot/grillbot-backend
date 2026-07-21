using GrillBot.Contracts.ImageProcessing;
using ImageMagick;
using ImageProcessingService.Extensions;

namespace ImageProcessingService.Models.Extensions;

/// <summary>
/// Frame decoding used to hang off PeepoRequest itself, which dragged ImageMagick into the
/// shared contract. The request is now data only and the rendering concern stays here.
/// </summary>
public static class PeepoRequestExtensions
{
    public static bool IsAnimated(this PeepoRequest request)
        => request.AvatarInfo.IsAnimated(request.GuildUploadLimit);

    public static List<IMagickImage<byte>> GetProfilePictureFrames(this PeepoRequest request)
        => request.AvatarInfo.GetProfilePictureFrames(request.GuildUploadLimit).ToList();
}
