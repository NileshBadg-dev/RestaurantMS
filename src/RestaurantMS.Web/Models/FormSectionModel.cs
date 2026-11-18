namespace RestaurantMS.Web.Models;

public class FormSectionModel
{
    public string Title { get; init; } = string.Empty;
    public string? HelperText { get; init; }
    public string? Icon { get; init; }
    public int? StepNumber { get; init; }
}
