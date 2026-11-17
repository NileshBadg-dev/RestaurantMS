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

        var isSuperUser = _currentUser.Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserId, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "admin", StringComparison.OrdinalIgnoreCase);

        if (!isSuperUser && !_currentUser.Permissions.Contains(Permission, StringComparer.OrdinalIgnoreCase))
        {
            output.SuppressOutput();
        }
    }
}
