namespace RestaurantMS.Web.Models;

public class MasterDrawerFormViewModel
{
    public string Tab { get; init; } = string.Empty;
    public string SectionTitle { get; init; } = string.Empty;
    public string? SectionHelperText { get; init; }
    public string Icon { get; init; } = "bi-pencil-square";
    public IReadOnlyList<FormFieldModel> Fields { get; init; } = [];
}
