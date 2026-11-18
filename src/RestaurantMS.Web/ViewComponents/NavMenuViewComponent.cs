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
                    if (!isEnabled) return false;
                }
                catch
                {
                    // Fallback to true if cache/db toggle unavailable
                }
            }

            return true;
        }

        async Task<List<NavItem>> FilterAllowedChildrenAsync(IEnumerable<NavItem> items)
        {
            var allowed = new List<NavItem>();
            foreach (var item in items)
            {
                if (await IsItemAllowedAsync(item))
                {
                    allowed.Add(item);
                }
            }
            return allowed;
        }

        var visibleItems = new List<NavItem>();

        // 1. Dashboard
        var dashboardItem = new NavItem
        {
            TitleKey = "Nav_Dashboard",
            Controller = "Dashboard",
            Action = "Index",
            Icon = "bi-speedometer2",
            RequiredPermission = Permissions.Dashboard.View
        };
        if (await IsItemAllowedAsync(dashboardItem))
        {
            visibleItems.Add(dashboardItem);
        }

        // 2. OPERATIONS Section
        var operationsItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Tables", Controller = "Tables", Action = "Index", Icon = "bi-grid-3x3-gap", RequiredPermission = Permissions.Tables.View, FeatureKey = FeatureKeys.Tables },
            new() { TitleKey = "Nav_Parcel", Controller = "Parcel", Action = "Index", Icon = "bi-box-seam", RequiredPermission = Permissions.Parcel.View },
            new() { TitleKey = "Nav_Orders", Controller = "Orders", Action = "Index", Icon = "bi-receipt-cutoff", RequiredPermission = Permissions.Orders.View },
            new() { TitleKey = "Nav_Kitchen", Controller = "Kitchen", Action = "Index", Icon = "bi-fire", RequiredPermission = Permissions.Kitchen.View, FeatureKey = FeatureKeys.Kitchen },
            new() { TitleKey = "Nav_Billing", Controller = "Billing", Action = "Index", Icon = "bi-cash-coin", RequiredPermission = Permissions.Billing.View },
            new() { TitleKey = "Nav_Reservations", Controller = "Reservations", Action = "Index", Icon = "bi-calendar-check", RequiredPermission = Permissions.Reservations.View }
        };

        var allowedOperations = await FilterAllowedChildrenAsync(operationsItems);
        if (allowedOperations.Count > 0)
        {
            visibleItems.Add(new NavItem { TitleKey = "Nav_Section_Operations", IsSectionHeader = true });
            visibleItems.AddRange(allowedOperations);
        }

        // 3. Masters (Single tabbed entry)
        var mastersItem = new NavItem
        {
            TitleKey = "Nav_Masters",
            Controller = "Masters",
            Action = "Index",
            Icon = "bi-stack",
            RequiredPermission = Permissions.Masters.View
        };
        if (await IsItemAllowedAsync(mastersItem))
        {
            visibleItems.Add(mastersItem);
        }

        // 4. Inventory (Collapsible Group — honours Inventory.Enabled toggle)
        var inventoryGroupShell = new NavItem
        {
            TitleKey = "Nav_Inventory",
            Controller = "Inventory",
            Action = "Index",
            Icon = "bi-boxes",
            RequiredPermission = Permissions.Inventory.View,
            FeatureKey = FeatureKeys.Inventory
        };
        if (await IsItemAllowedAsync(inventoryGroupShell))
        {
            var inventorySubItems = new List<NavItem>
            {
                new() { TitleKey = "Nav_StockSummary", Controller = "Inventory", Action = "Index", Icon = "bi-boxes", RequiredPermission = Permissions.Inventory.View, FeatureKey = FeatureKeys.Inventory },
                new() { TitleKey = "Nav_Purchase", Controller = "Inventory", Action = "Purchase", Icon = "bi-cart-plus", RequiredPermission = Permissions.Inventory.Purchase, FeatureKey = FeatureKeys.Inventory },
                new() { TitleKey = "Nav_StockAdjustment", Controller = "Inventory", Action = "Adjustment", Icon = "bi-sliders", RequiredPermission = Permissions.Inventory.Adjust, FeatureKey = FeatureKeys.Inventory },
                new() { TitleKey = "Nav_Wastage", Controller = "Inventory", Action = "Wastage", Icon = "bi-trash3", RequiredPermission = Permissions.Inventory.Wastage, FeatureKey = FeatureKeys.Inventory },
                new() { TitleKey = "Nav_Consumption", Controller = "Inventory", Action = "Consumption", Icon = "bi-fire", RequiredPermission = Permissions.Inventory.Consumption, FeatureKey = FeatureKeys.Inventory },
                new() { TitleKey = "Nav_LowStockAlerts", Controller = "Inventory", Action = "LowStock", Icon = "bi-exclamation-triangle", RequiredPermission = Permissions.Inventory.View, FeatureKey = FeatureKeys.Inventory },
                new() { TitleKey = "Nav_StockLedger", Controller = "Inventory", Action = "Ledger", Icon = "bi-journal-check", RequiredPermission = Permissions.Inventory.View, FeatureKey = FeatureKeys.Inventory }
            };
            var allowedInventory = await FilterAllowedChildrenAsync(inventorySubItems);
            if (allowedInventory.Count > 0)
            {
                visibleItems.Add(new NavItem
                {
                    TitleKey = inventoryGroupShell.TitleKey,
                    Controller = inventoryGroupShell.Controller,
                    Action = inventoryGroupShell.Action,
                    Icon = inventoryGroupShell.Icon,
                    RequiredPermission = inventoryGroupShell.RequiredPermission,
                    FeatureKey = inventoryGroupShell.FeatureKey,
                    Children = allowedInventory
                });
            }
        }

        // 5. Staff (Collapsible Group)
        var staffSubItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Staff", Controller = "Staff", Action = "Index", Icon = "bi-person-badge", RequiredPermission = Permissions.Staff.View },
            new() { TitleKey = "Nav_StaffAttendance", Controller = "Staff", Action = "Attendance", Icon = "bi-calendar2-check", RequiredPermission = Permissions.Staff.Attendance },
            new() { TitleKey = "Nav_SalaryReport", Controller = "Staff", Action = "SalaryReport", Icon = "bi-file-earmark-spreadsheet", RequiredPermission = Permissions.Staff.Salary }
        };
        var allowedStaff = await FilterAllowedChildrenAsync(staffSubItems);
        if (allowedStaff.Count > 0)
        {
            visibleItems.Add(new NavItem
            {
                TitleKey = "Nav_Staff",
                Controller = "Staff",
                Action = "Index",
                Icon = "bi-person-badge",
                RequiredPermission = Permissions.Staff.View,
                Children = allowedStaff
            });
        }

        // 6. Accounts (Collapsible Group)
        var accountsSubItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Vouchers", Controller = "Accounts", Action = "Vouchers", Icon = "bi-journal-plus", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_CustomerLedger", Controller = "Customers", Action = "Ledger", Icon = "bi-people", RequiredPermission = Permissions.Credit.View, FeatureKey = FeatureKeys.Credit },
            new() { TitleKey = "Nav_StaffAdvances", Controller = "EmployeeAdvances", Action = "Index", Icon = "bi-cash-coin", RequiredPermission = Permissions.EmployeeAdvances.View },
            new() { TitleKey = "Nav_DayBook", Controller = "Accounts", Action = "DayBook", Icon = "bi-journal-text", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_CashBook", Controller = "Accounts", Action = "CashBook", Icon = "bi-wallet2", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_GeneralLedger", Controller = "Accounts", Action = "LedgerSummary", Icon = "bi-book", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_TrialBalance", Controller = "Accounts", Action = "TrialBalance", Icon = "bi-bar-chart", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_ProfitLoss", Controller = "Accounts", Action = "ProfitLoss", Icon = "bi-graph-up-arrow", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_BalanceSheet", Controller = "Accounts", Action = "BalanceSheet", Icon = "bi-scale", RequiredPermission = Permissions.Accounts.View },
            new() { TitleKey = "Nav_Expenses", Controller = "Accounts", Action = "Vouchers", RouteValueKey = "type", RouteValue = "Payment", Icon = "bi-receipt", RequiredPermission = Permissions.Accounts.View }
        };
        var allowedAccounts = await FilterAllowedChildrenAsync(accountsSubItems);
        if (allowedAccounts.Count > 0)
        {
            visibleItems.Add(new NavItem
            {
                TitleKey = "Nav_Accounts",
                Controller = "Accounts",
                Action = "Index",
                Icon = "bi-journal-bookmark",
                RequiredPermission = Permissions.Accounts.View,
                FeatureKey = FeatureKeys.Accounts,
                Children = allowedAccounts
            });
        }

        // 7. Reports
        var reportsItem = new NavItem
        {
            TitleKey = "Nav_Reports",
            Controller = "Reports",
            Action = "Index",
            Icon = "bi-bar-chart-line",
            RequiredPermission = Permissions.Reports.View
        };
        if (await IsItemAllowedAsync(reportsItem))
        {
            visibleItems.Add(reportsItem);
        }

        // 8. Day Closing
        var dayClosingItem = new NavItem
        {
            TitleKey = "Nav_DayClosing",
            Controller = "DayClosing",
            Action = "Index",
            Icon = "bi-clock-history",
            RequiredPermission = Permissions.DayClosing.View
        };
        if (await IsItemAllowedAsync(dayClosingItem))
        {
            visibleItems.Add(dayClosingItem);
        }

        // 9. ADMINISTRATION Section
        var adminItems = new List<NavItem>
        {
            new() { TitleKey = "Nav_Settings", Controller = "Settings", Action = "Index", Icon = "bi-gear", RequiredPermission = Permissions.Settings.View },
            new() { TitleKey = "Nav_Users", Controller = "Users", Action = "Index", Icon = "bi-person-gear", RequiredPermission = Permissions.Users.View },
            new() { TitleKey = "Nav_Roles", Controller = "Roles", Action = "Index", Icon = "bi-shield-lock", RequiredPermission = Permissions.Roles.View },
            new() { TitleKey = "Nav_FeatureToggles", Controller = "FeatureToggles", Action = "Index", Icon = "bi-toggle-on", RequiredPermission = Permissions.FeatureToggles.View }
        };
        var allowedAdmin = await FilterAllowedChildrenAsync(adminItems);
        if (allowedAdmin.Count > 0)
        {
            visibleItems.Add(new NavItem { TitleKey = "Nav_Section_Administration", IsSectionHeader = true });
            visibleItems.AddRange(allowedAdmin);
        }

        return View(visibleItems);
    }
}
