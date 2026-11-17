using RestaurantMS.SharedKernel;

namespace RestaurantMS.Domain.Entities;

public class FeatureToggle : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
