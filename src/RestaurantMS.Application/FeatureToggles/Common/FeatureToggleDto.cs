namespace RestaurantMS.Application.FeatureToggles.Common;

public class FeatureToggleDto
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
