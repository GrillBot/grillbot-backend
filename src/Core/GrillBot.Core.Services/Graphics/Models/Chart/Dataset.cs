namespace Graphics.Models.Chart;

public class Dataset
{
    public string Label { get; set; } = null!;
    public List<DataPoint> Data { get; set; } = [];
    public string? Color { get; set; }
    public int Width { get; set; }
}
