using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RestaurantMS.Web.TagHelpers;

[HtmlTargetElement("date-range-picker")]
public class DateRangePickerTagHelper : TagHelper
{
    [HtmlAttributeName("start-for")]
    public ModelExpression? StartFor { get; set; }

    [HtmlAttributeName("end-for")]
    public ModelExpression? EndFor { get; set; }

    [HtmlAttributeName("start-name")]
    public string? StartName { get; set; } = "StartDate";

    [HtmlAttributeName("end-name")]
    public string? EndName { get; set; } = "EndDate";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "d-flex flex-wrap align-items-center gap-2 date-range-picker-container");

        var sName = StartFor?.Name ?? StartName ?? "StartDate";
        var sId = StartFor != null ? TagBuilder.CreateSanitizedId(StartFor.Name, "_") : sName;
        var sVal = StartFor?.Model != null ? string.Format("{0:yyyy-MM-dd}", StartFor.Model) : DateTime.UtcNow.ToString("yyyy-MM-dd");

        var eName = EndFor?.Name ?? EndName ?? "EndDate";
        var eId = EndFor != null ? TagBuilder.CreateSanitizedId(EndFor.Name, "_") : eName;
        var eVal = EndFor?.Model != null ? string.Format("{0:yyyy-MM-dd}", EndFor.Model) : DateTime.UtcNow.ToString("yyyy-MM-dd");

        output.Content.SetHtmlContent($@"
            <div class=""input-group input-group-sm"" style=""max-width: 170px;"">
                <span class=""input-group-text bg-white""><i class=""bi bi-calendar3 text-muted""></i></span>
                <input type=""date"" id=""{sId}"" name=""{sName}"" value=""{sVal}"" class=""form-control form-control-sm"" />
            </div>
            <span class=""text-muted small"">to</span>
            <div class=""input-group input-group-sm"" style=""max-width: 170px;"">
                <span class=""input-group-text bg-white""><i class=""bi bi-calendar3 text-muted""></i></span>
                <input type=""date"" id=""{eId}"" name=""{eName}"" value=""{eVal}"" class=""form-control form-control-sm"" />
            </div>");
    }
}
