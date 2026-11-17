using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.ViewComponents;

public class NavMenuViewComponent : ViewComponent
{
    private readonly ICurrentUser _currentUser;
    private readonly IFeatureService _featureService;

    public NavMenuViewComponent(ICurrentUser currentUser, IFeatureService featureService)
    {
        _currentUser = currentUser;
        _featureService = featureService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Dashboard", Controller = "Home", Action = "Index", Icon = "bi-speedometer2", RequiredPermission = Permissions.Dashboard.View, Category = "Overview" },
            new() { TitleKey = "Nav_Users", Controller = "Users", Action = "Index", Icon = "bi-people", RequiredPermission = Permissions.Users.View, Category = "Administration" },
            new() { TitleKey = "Nav_Roles", Controller = "Roles", Action = "Index", Icon = "bi-shield-lock", RequiredPermission = Permissions.Roles.View, Category = "Administration" },
            new() { TitleKey = "Nav_Settings", Controller = "Settings", Action = "Index", Icon = "bi-gear", RequiredPermission = Permissions.Settings.View, Category = "Configuration" },
            new() { TitleKey = "Nav_FeatureToggles", Controller = "FeatureToggles", Action = "Index", Icon = "bi-toggle-on", RequiredPermission = Permissions.FeatureToggles.View, Category = "Configuration" }
        };

        var visibleItems = new List<NavItem>();
        foreach (var item in allItems)
        {
            var hasPermission = string.IsNullOrEmpty(item.RequiredPermission) ||
                                _currentUser.Permissions.Contains(item.RequiredPermission, StringComparer.OrdinalIgnoreCase);

            if (!hasPermission)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(item.FeatureKey))
            {
                var isEnabled = await _featureService.IsEnabledAsync(item.FeatureKey);
                if (!isEnabled)
                {
                    continue;
                }
            }

            visibleItems.Add(item);
        }

        return View(visibleItems);
    }
}
