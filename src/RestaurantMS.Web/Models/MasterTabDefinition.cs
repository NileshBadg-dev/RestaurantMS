namespace RestaurantMS.Web.Models;

public class MasterTabDefinition
{
    public string Id { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string CountBadge { get; init; } = string.Empty;
    public string RequiredPermission { get; init; } = string.Empty;
    public string? FeatureKey { get; init; }
    public string PrimaryActionKey { get; init; } = string.Empty;
    public string? SecondaryActionKey { get; init; }
}
