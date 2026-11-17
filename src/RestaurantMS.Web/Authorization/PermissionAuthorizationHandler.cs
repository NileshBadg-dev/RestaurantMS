using Microsoft.AspNetCore.Authorization;

namespace RestaurantMS.Web.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var isSuperUser = context.User.IsInRole("Owner") ||
                          context.User.IsInRole("Admin") ||
                          string.Equals(context.User.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, "admin", StringComparison.OrdinalIgnoreCase);

        var hasPermission = isSuperUser || context.User.Claims
            .Any(c => c.Type == "Permission" && c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
