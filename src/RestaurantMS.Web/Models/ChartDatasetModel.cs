namespace RestaurantMS.Web.Models;

public class ChartDatasetModel
{
    public string Label { get; set; } = string.Empty;
    public List<decimal> Data { get; set; } = new();
    public string BackgroundColor { get; set; } = "#3D1F6D";
    public string BorderColor { get; set; } = "#3D1F6D";
    public int BorderWidth { get; set; } = 1;
    public bool Fill { get; set; } = false;
    public string? Stack { get; set; }
}
