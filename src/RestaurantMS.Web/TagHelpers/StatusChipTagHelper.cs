using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RestaurantMS.Web.TagHelpers;

[HtmlTargetElement("status-chip")]
public class StatusChipTagHelper : TagHelper
{
    [HtmlAttributeName("status")]
    public string? Status { get; set; }

    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        var statusNorm = (Status ?? string.Empty).ToLowerInvariant().Trim();
        var (cls, dotCls) = statusNorm switch
        {
            "free" or "available" or "active" or "ready" or "success" or "paid" or "settled" => ("chip-free", "bg-teal"),
            "occupied" or "busy" or "danger" or "late" or "cancelled" or "overdue" => ("chip-occupied", "bg-chili"),
            "reserved" or "warning" or "preparing" or "pending" or "partiallypaid" => ("chip-reserved", "bg-saffron"),
            "cleaning" or "billed" or "secondary" or "inactive" or "draft" => ("chip-cleaning", "bg-muted"),
            _ => ("chip-cleaning", "bg-secondary")
        };

        output.Attributes.SetAttribute("class", $"status-chip {cls}");
        var text = !string.IsNullOrWhiteSpace(Label) ? Label : Status ?? "Unknown";

        output.Content.SetHtmlContent($@"<span class=""status-dot {dotCls}""></span><span>{text}</span>");
    }
}
