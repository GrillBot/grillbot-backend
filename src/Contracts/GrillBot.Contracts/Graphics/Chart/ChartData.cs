namespace GrillBot.Contracts.Graphics.Chart;

public class ChartData
{
    public Label? TopLabel { get; set; }
    public List<Dataset> Datasets { get; set; } = [];
}
