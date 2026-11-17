namespace RestaurantMS.Web.Models;

public class KpiCardModel
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Theme { get; set; } = "saffron"; // saffron, plum, chili, teal, ink
    public string? Icon { get; set; }

    public KpiCardModel() { }

    public KpiCardModel(string title, string value, string subtitle, string theme = "saffron", string? icon = null)
    {
        Title = title;
        Value = value;
        Subtitle = subtitle;
        Theme = theme;
        Icon = icon;
    }
}
