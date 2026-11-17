namespace RestaurantMS.Web.Models;

public class MasterItemModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#3D1F6D";
    public string BadgeText { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string RequiredPermission { get; set; } = string.Empty;
}
