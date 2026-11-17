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
        var isSuperUser = _currentUser.Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserId, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "System Administrator", StringComparison.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Count == 0;

        async Task<bool> IsItemAllowedAsync(NavItem item)
        {
            var hasPermission = isSuperUser ||
                                string.IsNullOrEmpty(item.RequiredPermission) ||
                                _currentUser.Permissions.Contains(item.RequiredPermission, StringComparer.OrdinalIgnoreCase) ||
                                _currentUser.Permissions.Count == 0;

            if (!hasPermission) return false;

            if (!string.IsNullOrEmpty(item.FeatureKey))
            {
                try
                {
                    var isEnabled = await _featureService.IsEnabledAsync(item.FeatureKey);
                    if (!isEnabled && !isSuperUser) return false;
                }
                catch
                {
                    // Fallback to true if cache/db toggle unavailable
                }
            }

            return true;
        }

        var masterSubItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Menu", Controller = "Menu", Action = "Categories", Icon = "bi-card-list", RequiredPermission = Permissions.Menu.View },
            new() { TitleKey = "Nav_Customers", Controller = "Customers", Action = "Index", Icon = "bi-people", RequiredPermission = Permissions.Customers.View, FeatureKey = FeatureKeys.Credit },
            new() { TitleKey = "Nav_Inventory", Controller = "Inventory", Action = "Index", Icon = "bi-boxes", RequiredPermission = Permissions.Inventory.View },
            new() { TitleKey = "Nav_Suppliers", Controller = "Inventory", Action = "Suppliers", Icon = "bi-truck", RequiredPermission = Permissions.Inventory.View },
            new() { TitleKey = "Nav_Staff", Controller = "Staff", Action = "Index", Icon = "bi-person-badge", RequiredPermission = Permissions.Staff.View },
            new() { TitleKey = "Nav_StaffAttendance", Controller = "Staff", Action = "Attendance", Icon = "bi-calendar2-check", RequiredPermission = Permissions.Staff.View },
            new() { TitleKey = "Nav_StaffAdvances", Controller = "Staff", Action = "Advances", Icon = "bi-cash-coin", RequiredPermission = Permissions.Staff.View },
            new() { TitleKey = "Nav_SalaryReport", Controller = "Staff", Action = "SalaryReport", Icon = "bi-file-earmark-spreadsheet", RequiredPermission = Permissions.Staff.View },
            new() { TitleKey = "Nav_Accounts", Controller = "Accounts", Action = "Index", Icon = "bi-journal-bookmark", RequiredPermission = Permissions.Accounts.View, FeatureKey = FeatureKeys.Accounts },
            new() { TitleKey = "Nav_Reservations", Controller = "Reservations", Action = "Index", Icon = "bi-calendar-check", RequiredPermission = Permissions.Reservations.View },
            new() { TitleKey = "Nav_Settings", Controller = "Settings", Action = "Index", Icon = "bi-gear", RequiredPermission = Permissions.Settings.View },
            new() { TitleKey = "Nav_Users", Controller = "Users", Action = "Index", Icon = "bi-person-gear", RequiredPermission = Permissions.Users.View },
            new() { TitleKey = "Nav_Roles", Controller = "Roles", Action = "Index", Icon = "bi-shield-lock", RequiredPermission = Permissions.Roles.View },
            new() { TitleKey = "Nav_FeatureToggles", Controller = "FeatureToggles", Action = "Index", Icon = "bi-toggle-on", RequiredPermission = Permissions.FeatureToggles.View }
        };

        var allowedMasterChildren = new List<NavItem>();
        foreach (var sub in masterSubItems)
        {
            if (await IsItemAllowedAsync(sub))
            {
                allowedMasterChildren.Add(sub);
            }
        }

        var allItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Dashboard", Controller = "Dashboard", Action = "Index", Icon = "bi-speedometer2", RequiredPermission = Permissions.Dashboard.View },
            new() { TitleKey = "Nav_Tables", Controller = "Tables", Action = "Index", Icon = "bi-grid-3x3-gap", RequiredPermission = Permissions.Tables.View, FeatureKey = FeatureKeys.Tables },
            new() { TitleKey = "Nav_Parcel", Controller = "Parcel", Action = "Index", Icon = "bi-box-seam", RequiredPermission = Permissions.Parcel.View },
            new() { TitleKey = "Nav_Orders", Controller = "Orders", Action = "Index", Icon = "bi-receipt-cutoff", RequiredPermission = Permissions.Orders.View },
            new() { TitleKey = "Nav_OnlineOrders", Controller = "OnlineOrders", Action = "Index", Icon = "bi-globe2", RequiredPermission = Permissions.OnlineOrders.View },
            new() { TitleKey = "Nav_Kitchen", Controller = "Kitchen", Action = "Index", Icon = "bi-fire", RequiredPermission = Permissions.Kitchen.View, FeatureKey = FeatureKeys.Kitchen },
            new() { TitleKey = "Nav_Billing", Controller = "Billing", Action = "Index", Icon = "bi-cash-coin", RequiredPermission = Permissions.Billing.View },
            new() { TitleKey = "Nav_Inventory", Controller = "Inventory", Action = "Index", Icon = "bi-boxes", RequiredPermission = Permissions.Inventory.View },
            new() { TitleKey = "Nav_DayClosing", Controller = "DayClosing", Action = "Index", Icon = "bi-clock-history", RequiredPermission = Permissions.DayClosing.View },
            new() { TitleKey = "Nav_Reports", Controller = "Reports", Action = "Index", Icon = "bi-bar-chart-line", RequiredPermission = Permissions.Reports.View }
        };

        var visibleItems = new List<NavItem>();
        foreach (var item in allItems)
        {
            if (await IsItemAllowedAsync(item))
            {
                visibleItems.Add(item);
            }
        }

        if (allowedMasterChildren.Count > 0 || isSuperUser)
        {
            visibleItems.Add(new NavItem
            {
                TitleKey = "Nav_Masters",
                Controller = "Masters",
                Action = "Index",
                Icon = "bi-stack",
                Children = allowedMasterChildren
            });
        }

        return View(visibleItems);
    }
}
