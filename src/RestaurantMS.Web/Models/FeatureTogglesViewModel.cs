using RestaurantMS.Application.FeatureToggles.Common;

namespace RestaurantMS.Web.Models;

public class FeatureTogglesViewModel
{
    public IReadOnlyList<FeatureToggleDto> Toggles { get; set; } = [];
    public Dictionary<string, bool> ToggleStates { get; set; } = [];
}
