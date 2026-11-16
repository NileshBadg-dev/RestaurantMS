using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RestaurantMS.Web.TagHelpers;

[HtmlTargetElement("money-input", Attributes = "asp-for")]
public class MoneyInputTagHelper : TagHelper
{
    private readonly IHtmlGenerator _generator;

    public MoneyInputTagHelper(IHtmlGenerator generator)
    {
        _generator = generator;
    }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;

    [HtmlAttributeName("currency-symbol")]
    public string CurrencySymbol { get; set; } = "₹";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "input-group");

        var span = new TagBuilder("span");
        span.AddCssClass("input-group-text");
        span.InnerHtml.Append(CurrencySymbol);

        var input = _generator.GenerateTextBox(
            ViewContext,
            For.ModelExplorer,
            For.Name,
            For.Model,
            "{0:F2}",
            new
            {
                @class = "form-control text-end",
                type = "number",
                step = "0.01",
                placeholder = "0.00"
            });

        output.Content.AppendHtml(span);
        output.Content.AppendHtml(input);
    }
}
