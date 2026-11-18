namespace RestaurantMS.Web.Models;

public class DrawerFormModel
{
    public string DrawerId { get; init; } = "appGlobalDrawer";
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string Icon { get; init; } = "bi-pencil-square";
    public string Width { get; init; } = "520px";
}
