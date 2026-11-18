namespace RestaurantMS.Web.Models;

public class FormFooterModel
{
    public string CancelUrl { get; init; } = "/";
    public string SaveLabelKey { get; init; } = "Common_Save";
    public string SaveAndNewLabelKey { get; init; } = "Form_SaveAndNew";
    public string CancelLabelKey { get; init; } = "Common_Cancel";
    public bool ShowSaveAndNew { get; init; } = true;
    public bool IsDrawer { get; init; }
    public string? PrimaryButtonIcon { get; init; } = "bi-check2-circle";
}
