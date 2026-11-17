using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RestaurantMS.Web.TagHelpers;

[HtmlTargetElement("mobile-input")]
public class MobileInputTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("placeholder")]
    public string? Placeholder { get; set; } = "9876543210";

    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "input-group mobile-input-group");

        var name = For?.Name ?? "mobile";
        var id = Id ?? (For != null ? TagBuilder.CreateSanitizedId(For.Name, "_") : name);
        var val = For?.Model?.ToString() ?? string.Empty;
        var disabledAttr = Disabled ? "disabled" : "";

        output.Content.SetHtmlContent($@"
            <span class=""input-group-text bg-light border-end-0"">
                <i class=""bi bi-telephone text-muted""></i>
                <span class=""ms-1 text-muted small fw-semibold"">+91</span>
            </span>
            <input type=""tel"" id=""{id}"" name=""{name}"" class=""form-control border-start-0"" value=""{val}""
                   placeholder=""{Placeholder}"" maxlength=""10"" pattern=""^[6-9]\d{{9}}$""
                   inputmode=""numeric"" {disabledAttr} />");
    }
}
