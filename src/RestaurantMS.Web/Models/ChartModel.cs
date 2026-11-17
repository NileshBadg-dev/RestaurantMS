namespace RestaurantMS.Web.Models;

public class ChartModel
{
    public string ChartId { get; set; } = "chart_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    public string Type { get; set; } = "bar"; // bar, line, stackedBar
    public string Title { get; set; } = string.Empty;
    public int HeightPx { get; set; } = 300;
    public List<string> Labels { get; set; } = new();
    public List<ChartDatasetModel> Datasets { get; set; } = new();
}
