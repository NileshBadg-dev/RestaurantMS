using Microsoft.AspNetCore.Authorization;

namespace RestaurantMS.Web.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionAuthorizeAttribute(string permission)
    {
        Policy = $"Permission:{permission}";
    }
}
