namespace RestaurantMS.Web.Models;

public class NavItem
{
    public string TitleKey { get; init; } = string.Empty;
    public string Controller { get; init; } = string.Empty;
    public string Action { get; init; } = "Index";
    public string Icon { get; init; } = string.Empty;
    public string RequiredPermission { get; init; } = string.Empty;
    public string? FeatureKey { get; init; }
    public string Category { get; init; } = "General";
    public List<NavItem> Children { get; init; } = new();
}
