using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RestaurantMS.Web.TagHelpers;

[HtmlTargetElement("quantity-input")]
public class QuantityInputTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("min")]
    public int Min { get; set; } = 1;

    [HtmlAttributeName("max")]
    public int Max { get; set; } = 999;

    [HtmlAttributeName("step")]
    public int Step { get; set; } = 1;

    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "input-group quantity-input-group");

        var name = For?.Name ?? "quantity";
        var id = Id ?? (For != null ? TagBuilder.CreateSanitizedId(For.Name, "_") : name);
        var val = For?.Model?.ToString() ?? Min.ToString();

        output.Content.SetHtmlContent($@"
            <button class=""btn btn-outline-secondary btn-qty-minus"" type=""button"" onclick=""const input = document.getElementById('{id}'); input.value = Math.max({Min}, (parseInt(input.value) || {Min}) - {Step}); input.dispatchEvent(new Event('change'));"">
                <i class=""bi bi-dash""></i>
            </button>
            <input type=""number"" id=""{id}"" name=""{name}"" class=""form-control text-center input-qty"" value=""{val}"" min=""{Min}"" max=""{Max}"" step=""{Step}"" />
            <button class=""btn btn-outline-secondary btn-qty-plus"" type=""button"" onclick=""const input = document.getElementById('{id}'); input.value = Math.min({Max}, (parseInt(input.value) || {Min}) + {Step}); input.dispatchEvent(new Event('change'));"">
                <i class=""bi bi-plus""></i>
            </button>");
    }
}
