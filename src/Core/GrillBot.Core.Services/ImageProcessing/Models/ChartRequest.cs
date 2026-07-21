using Graphics.Models.Chart;

namespace ImageProcessing.Models;

public class ChartRequest
{
    public List<ChartRequestData> Requests { get; set; } = [];
}
