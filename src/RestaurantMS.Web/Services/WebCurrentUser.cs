using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RestaurantMS.Application.Common.Interfaces;
using PermissionsConstants = RestaurantMS.Domain.Constants.Permissions;

namespace RestaurantMS.Web.Services;

public class WebCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("UserId")?.Value;

    public string? UserName => User?.Identity?.Name
                            ?? User?.FindFirst(ClaimTypes.Name)?.Value;

    public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role)
                                              .Select(c => c.Value)
                                              .Distinct()
                                              .ToList()
                                          ?? (IReadOnlyList<string>)Array.Empty<string>();

    public IReadOnlyList<string> Permissions
    {
        get
        {
            var isSuperUser = Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                              Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                              string.Equals(UserId, "admin", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(UserName, "admin", StringComparison.OrdinalIgnoreCase);

            if (isSuperUser)
            {
                return PermissionsConstants.GetAll();
            }

            return User?.FindAll("Permission")
                       .Select(c => c.Value)
                       .Distinct()
                       .ToList() ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
