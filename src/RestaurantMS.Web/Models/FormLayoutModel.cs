namespace RestaurantMS.Web.Models;

public class FormLayoutModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public bool ShowSaveAndNew { get; set; } = true;
    public string SaveButtonText { get; set; } = "Save";
    public string SaveAndNewButtonText { get; set; } = "Save & New";
    public string CancelButtonText { get; set; } = "Cancel";
    public bool IsMultipart { get; set; } = false;
}
