using Microsoft.AspNetCore.Razor.TagHelpers;
using RestaurantMS.Application.Common.Interfaces;

namespace RestaurantMS.Web.TagHelpers;

[HtmlTargetElement(Attributes = "permission")]
public class PermissionTagHelper : TagHelper
{
    private readonly ICurrentUser _currentUser;

    public PermissionTagHelper(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HtmlAttributeName("permission")]
    public string? Permission { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Permission))
        {
            return;
        }

        if (!_currentUser.Permissions.Contains(Permission, StringComparer.OrdinalIgnoreCase))
        {
            output.SuppressOutput();
        }
    }
}
