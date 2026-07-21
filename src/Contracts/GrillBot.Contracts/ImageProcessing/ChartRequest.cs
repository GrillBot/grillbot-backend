using GrillBot.Contracts.Graphics.Chart;
using GrillBot.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Contracts.ImageProcessing;

public class ChartRequest
{
    [Required]
    [RequireSomeItemInCollection(ErrorMessage = "Some request is required.")]
    public List<ChartRequestData> Requests { get; set; } = [];
}
