namespace RestaurantMS.Web.Models;

public class EmptyStateModel
{
    public string Title { get; set; } = "No records found";
    public string Description { get; set; } = "There is no data to display for the current selection.";
    public string Icon { get; set; } = "bi-inbox";
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }

    public EmptyStateModel() { }

    public EmptyStateModel(string title, string description, string icon = "bi-inbox", string? actionUrl = null, string? actionText = null)
    {
        Title = title;
        Description = description;
        Icon = icon;
        ActionUrl = actionUrl;
        ActionText = actionText;
    }
}
