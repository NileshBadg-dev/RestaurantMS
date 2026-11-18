namespace RestaurantMS.Web.Models;

public class NavItem
{
    public string TitleKey { get; init; } = string.Empty;
    public string Controller { get; init; } = string.Empty;
    public string Action { get; init; } = "Index";
    public string? RouteValueKey { get; init; }
    public string? RouteValue { get; init; }
    public string Icon { get; init; } = string.Empty;
    public string RequiredPermission { get; init; } = string.Empty;
    public string? FeatureKey { get; init; }
    public bool IsSectionHeader { get; init; }
    public List<NavItem> Children { get; init; } = new();
}
