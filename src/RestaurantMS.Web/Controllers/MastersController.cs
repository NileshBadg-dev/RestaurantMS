using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class MastersController : Controller
{
    private readonly IMastersUiData _mastersUiData;
    private readonly ICurrentUser _currentUser;
    private readonly IFeatureService _featureService;

    public MastersController(
        IMastersUiData mastersUiData,
        ICurrentUser currentUser,
        IFeatureService featureService)
    {
        _mastersUiData = mastersUiData;
        _currentUser = currentUser;
        _featureService = featureService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? tab = null, string? category = null, CancellationToken cancellationToken = default)
    {
        var model = await BuildMastersViewModelAsync(tab, category, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> TabContent(string tab, string? category = null, CancellationToken cancellationToken = default)
    {
        var model = await BuildMastersViewModelAsync(tab, category, cancellationToken);
        return PartialView("_TabContent", model);
    }

    [HttpGet]
    public IActionResult DrawerForm(string tab, long? id = null)
    {
        var vm = BuildDrawerFormViewModel((tab ?? "categories").Trim().ToLowerInvariant(), id);
        return PartialView("_MasterDrawerForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMasterEntry(string tab, CancellationToken cancellationToken = default)
    {
        var fields = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
        var normalizedTab = (tab ?? "categories").Trim().ToLowerInvariant();
        var requiredField = normalizedTab switch
        {
            "tables" => "TableCode",
            "designations" => "Title",
            "customers" => "FullName",
            "categories" => "NameEn",
            _ => "Name"
        };

        if (!fields.TryGetValue(requiredField, out var primaryVal) || string.IsNullOrWhiteSpace(primaryVal))
        {
            return Json(new { isSuccess = false, error = "Please fill in all required fields." });
        }

        var highlightKey = await _mastersUiData.SaveEntryAsync(normalizedTab, fields, cancellationToken);
        return Json(new
        {
            isSuccess = true,
            tab = normalizedTab,
            highlightKey,
            message = $"'{highlightKey}' saved successfully!"
        });
    }

    private static MasterDrawerFormViewModel BuildDrawerFormViewModel(string tab, long? id)
    {
        return tab switch
        {
            "floors" => new MasterDrawerFormViewModel
            {
                Tab = "floors",
                SectionTitle = id.HasValue ? "Edit Dining Floor" : "Floor & Section Details",
                SectionHelperText = "Define dining zone name, short code, table count and seating capacity.",
                Icon = "bi-building",
                Fields =
                [
                    new() { Name = "Name", Label = "Floor / Section Name", Placeholder = "e.g. Second Floor Banquet", Required = true, ColClass = "col-12" },
                    new() { Name = "Code", Label = "Short Code", Placeholder = "e.g. SF-BNQ", Required = true, ColClass = "col-6" },
                    new() { Name = "TableCount", Label = "Total Tables", InputType = "stepper", Value = "4", Min = "1", Max = "100", ColClass = "col-6" },
                    new() { Name = "SeatingCapacity", Label = "Seating Capacity", InputType = "number", Value = "16", UnitSuffix = "seats", Min = "1", ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Active Floor", InputType = "toggle", Checked = true, HelperText = "Show in floor plan", ColClass = "col-6" },
                    new() { Name = "Description", Label = "Description", InputType = "textarea", Placeholder = "e.g. Air-conditioned family hall", ColClass = "col-12" }
                ]
            },
            "tables" => new MasterDrawerFormViewModel
            {
                Tab = "tables",
                SectionTitle = id.HasValue ? "Edit Dining Table" : "Dining Table Setup",
                SectionHelperText = "Assign table code, dining floor, seating capacity and shape.",
                Icon = "bi-grid-3x3-gap",
                Fields =
                [
                    new() { Name = "TableCode", Label = "Table Code", Placeholder = "e.g. T10", Required = true, ColClass = "col-6" },
                    new() { Name = "Capacity", Label = "Capacity", InputType = "stepper", Value = "4", Min = "1", Max = "24", UnitSuffix = "pax", ColClass = "col-6" },
                    new() { Name = "FloorName", Label = "Floor / Section", InputType = "select", Required = true, Options = [("Ground Floor AC Hall", "Ground Floor AC Hall"), ("First Floor Family Dining", "First Floor Family Dining"), ("Rooftop Terrace Garden", "Rooftop Terrace Garden")], ColClass = "col-12" },
                    new() { Name = "Shape", Label = "Table Shape", InputType = "select", Options = [("Square", "Square"), ("Round", "Round"), ("Rectangle", "Rectangle")], ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Available for Seating", InputType = "toggle", Checked = true, ColClass = "col-6" }
                ]
            },
            "categories" => new MasterDrawerFormViewModel
            {
                Tab = "categories",
                SectionTitle = id.HasValue ? "Edit Menu Category" : "Menu Category Details",
                SectionHelperText = "English and Marathi titles shown on POS menu tabs.",
                Icon = "bi-tags",
                Fields =
                [
                    new() { Name = "NameEn", Label = "Category Name (English)", Placeholder = "e.g. Tandoor Starters", Required = true, ColClass = "col-6" },
                    new() { Name = "NameMr", Label = "Category Name (मराठी)", Placeholder = "उदा. तंदूर स्टार्टर्स", Required = true, ColClass = "col-6" },
                    new() { Name = "DisplayOrder", Label = "Display Order", InputType = "stepper", Value = "6", Min = "1", Max = "99", ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Visible on POS", InputType = "toggle", Checked = true, HelperText = "Show on order screen", ColClass = "col-6" }
                ]
            },
            "customers" => new MasterDrawerFormViewModel
            {
                Tab = "customers",
                SectionTitle = id.HasValue ? "Edit Customer Profile" : "Customer & Udhari Profile",
                SectionHelperText = "Mobile and address are required for Udhari credit settlements.",
                Icon = "bi-people",
                Fields =
                [
                    new() { Name = "FullName", Label = "Customer Name", Placeholder = "e.g. Ramesh Patil", Required = true, ColClass = "col-6" },
                    new() { Name = "MobileNumber", Label = "Mobile Number", Placeholder = "10-digit mobile", Required = true, Pattern = "^[6-9]\\d{9}$", MaxLength = 10, ColClass = "col-6" },
                    new() { Name = "CreditLimit", Label = "Credit Limit", InputType = "number", Value = "5000", UnitSuffix = "₹", Min = "0", ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Allow Udhari Credit", InputType = "toggle", Checked = true, ColClass = "col-6" },
                    new() { Name = "Address", Label = "Address", Placeholder = "Flat / Road / Area, City", Required = true, ColClass = "col-12" }
                ]
            },
            "suppliers" => new MasterDrawerFormViewModel
            {
                Tab = "suppliers",
                SectionTitle = id.HasValue ? "Edit Supplier" : "Supplier / Vendor Profile",
                SectionHelperText = "Vendor contact info, GSTIN and material category.",
                Icon = "bi-truck",
                Fields =
                [
                    new() { Name = "Name", Label = "Supplier / Firm Name", Placeholder = "e.g. Sahyadri Fresh Produce", Required = true, ColClass = "col-12" },
                    new() { Name = "ContactPerson", Label = "Contact Person", Placeholder = "e.g. Santosh Pawar", ColClass = "col-6" },
                    new() { Name = "Phone", Label = "Mobile / Phone", Placeholder = "9823011223", Required = true, Pattern = "^[6-9]\\d{9}$", MaxLength = 10, ColClass = "col-6" },
                    new() { Name = "Gstin", Label = "GSTIN", Placeholder = "27AAAAA0000A1Z2", MaxLength = 15, ColClass = "col-6" },
                    new() { Name = "Category", Label = "Supply Category", InputType = "select", Options = [("Dairy & Grocery", "Dairy & Grocery"), ("Vegetables & Produce", "Vegetables & Produce"), ("Poultry & Non-Veg", "Poultry & Non-Veg"), ("Spices & Oils", "Spices & Oils")], ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Active Supplier", InputType = "toggle", Checked = true, ColClass = "col-12" }
                ]
            },
            "designations" => new MasterDrawerFormViewModel
            {
                Tab = "designations",
                SectionTitle = "Staff Designation Details",
                SectionHelperText = "Define role title, department and hierarchy level.",
                Icon = "bi-person-badge",
                Fields =
                [
                    new() { Name = "Title", Label = "Designation Title", Placeholder = "e.g. Sous Chef", Required = true, ColClass = "col-12" },
                    new() { Name = "Department", Label = "Department", InputType = "select", Options = [("Kitchen", "Kitchen"), ("Service", "Service"), ("Accounts", "Accounts"), ("Management", "Management")], ColClass = "col-6" },
                    new() { Name = "Level", Label = "Grade / Level", InputType = "select", Options = [("L1", "L1 — Entry"), ("L2", "L2 — Associate"), ("L3", "L3 — Lead"), ("L4", "L4 — Manager")], ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Active Role", InputType = "toggle", Checked = true, ColClass = "col-12" }
                ]
            },
            "shifts" => new MasterDrawerFormViewModel
            {
                Tab = "shifts",
                SectionTitle = "Work Shift Timing",
                SectionHelperText = "Configure shift start, end and grace period minutes.",
                Icon = "bi-clock",
                Fields =
                [
                    new() { Name = "Name", Label = "Shift Name", Placeholder = "e.g. Split Lunch & Dinner", Required = true, ColClass = "col-12" },
                    new() { Name = "StartTime", Label = "Start Time", Placeholder = "09:00 AM", Required = true, ColClass = "col-6" },
                    new() { Name = "EndTime", Label = "End Time", Placeholder = "06:00 PM", Required = true, ColClass = "col-6" },
                    new() { Name = "GracePeriodMinutes", Label = "Grace Period", InputType = "stepper", Value = "15", Min = "0", Max = "60", UnitSuffix = "min", ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Active Shift", InputType = "toggle", Checked = true, ColClass = "col-6" }
                ]
            },
            "tax-units" => new MasterDrawerFormViewModel
            {
                Tab = "tax-units",
                SectionTitle = "Tax Slab / Measurement Unit",
                SectionHelperText = "Add GST percentage slabs or kitchen inventory units.",
                Icon = "bi-percent",
                Fields =
                [
                    new() { Name = "Name", Label = "Name", Placeholder = "e.g. GST 12% or Gram (g)", Required = true, ColClass = "col-12" },
                    new() { Name = "Type", Label = "Type", InputType = "select", Options = [("Tax Slab", "Tax Slab"), ("Measurement Unit", "Measurement Unit")], ColClass = "col-6" },
                    new() { Name = "RateOrSymbol", Label = "Rate / Symbol", Placeholder = "e.g. 12% or g", Required = true, ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Active", InputType = "toggle", Checked = true, ColClass = "col-12" }
                ]
            },
            "payment-modes" => new MasterDrawerFormViewModel
            {
                Tab = "payment-modes",
                SectionTitle = "Payment Mode Setup",
                SectionHelperText = "Configure accepted payment modes and MDR surcharge.",
                Icon = "bi-credit-card",
                Fields =
                [
                    new() { Name = "Name", Label = "Mode Name", Placeholder = "e.g. Sodexo / Pluxee Meal Card", Required = true, ColClass = "col-12" },
                    new() { Name = "Code", Label = "Short Code", Placeholder = "e.g. PLUXEE", Required = true, ColClass = "col-6" },
                    new() { Name = "FeePercent", Label = "Surcharge / MDR", InputType = "number", Value = "0", Step = "0.1", Min = "0", Max = "10", UnitSuffix = "%", ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Enabled at POS", InputType = "toggle", Checked = true, ColClass = "col-12" }
                ]
            },
            _ => new MasterDrawerFormViewModel
            {
                Tab = "expense-heads",
                SectionTitle = "Expense Head Setup",
                SectionHelperText = "Categorize restaurant outflows and monthly budgets.",
                Icon = "bi-cash-stack",
                Fields =
                [
                    new() { Name = "Name", Label = "Expense Head Name", Placeholder = "e.g. Laundry & Linen", Required = true, ColClass = "col-12" },
                    new() { Name = "Category", Label = "Category", InputType = "select", Options = [("Operating", "Operating"), ("Facility", "Facility"), ("Payroll", "Payroll"), ("Maintenance", "Maintenance")], ColClass = "col-6" },
                    new() { Name = "MonthlyBudget", Label = "Monthly Budget", InputType = "number", Value = "15000", UnitSuffix = "₹", Min = "0", ColClass = "col-6" },
                    new() { Name = "IsTaxDeductible", Label = "Tax Deductible", InputType = "toggle", Checked = true, ColClass = "col-6" },
                    new() { Name = "IsActive", Label = "Active", InputType = "toggle", Checked = true, ColClass = "col-6" }
                ]
            }
        };
    }

    private async Task<MastersViewModel> BuildMastersViewModelAsync(string? requestedTab, string? category, CancellationToken cancellationToken)
    {
        var isSuperUser = _currentUser.Roles.Contains("Owner", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserId, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(_currentUser.UserName, "System Administrator", StringComparison.OrdinalIgnoreCase) ||
                          _currentUser.Roles.Count == 0;

        async Task<bool> IsTabAllowedAsync(string requiredPermission, string? featureKey)
        {
            var hasPerm = isSuperUser ||
                          string.IsNullOrEmpty(requiredPermission) ||
                          _currentUser.Permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase) ||
                          _currentUser.Permissions.Count == 0;

            if (!hasPerm) return false;

            if (!string.IsNullOrEmpty(featureKey))
            {
                try
                {
                    var isEnabled = await _featureService.IsEnabledAsync(featureKey, cancellationToken);
                    if (!isEnabled && !isSuperUser) return false;
                }
                catch
                {
                    // Fallback to true if toggle check unavailable
                }
            }

            return true;
        }

        var counts = await _mastersUiData.GetTabCountsAsync(cancellationToken);

        var allTabs = new List<MasterTabDefinition>
        {
            new() { Id = "floors", TitleKey = "Master_Floors", Icon = "bi-building", CountBadge = counts.FloorsCount.ToString(), RequiredPermission = Permissions.Tables.View, FeatureKey = FeatureKeys.Tables, PrimaryActionKey = "Master_AddFloor" },
            new() { Id = "tables", TitleKey = "Master_Tables", Icon = "bi-grid-3x3-gap", CountBadge = counts.TablesCount.ToString(), RequiredPermission = Permissions.Tables.View, FeatureKey = FeatureKeys.Tables, PrimaryActionKey = "Master_AddTable" },
            new() { Id = "categories", TitleKey = "Master_Categories", Icon = "bi-tags", CountBadge = counts.CategoriesCount.ToString(), RequiredPermission = Permissions.Menu.View, PrimaryActionKey = "Master_AddCategory" },
            new() { Id = "menu-items", TitleKey = "Master_MenuItems", Icon = "bi-egg-fried", CountBadge = counts.MenuItemsCount.ToString(), RequiredPermission = Permissions.Menu.View, PrimaryActionKey = "Master_AddMenuItem", SecondaryActionKey = "Master_AddCategory" },
            new() { Id = "customers", TitleKey = "Master_Customers", Icon = "bi-people", CountBadge = counts.CustomersCount.ToString(), RequiredPermission = Permissions.Customers.View, FeatureKey = FeatureKeys.Credit, PrimaryActionKey = "Master_AddCustomer" },
            new() { Id = "suppliers", TitleKey = "Master_Suppliers", Icon = "bi-truck", CountBadge = counts.SuppliersCount.ToString(), RequiredPermission = Permissions.Inventory.View, FeatureKey = FeatureKeys.Inventory, PrimaryActionKey = "Master_AddSupplier" },
            new() { Id = "designations", TitleKey = "Master_Designations", Icon = "bi-person-badge", CountBadge = counts.DesignationsCount.ToString(), RequiredPermission = Permissions.Staff.View, PrimaryActionKey = "Master_AddDesignation" },
            new() { Id = "shifts", TitleKey = "Master_Shifts", Icon = "bi-clock", CountBadge = counts.ShiftsCount.ToString(), RequiredPermission = Permissions.Staff.View, PrimaryActionKey = "Master_AddShift" },
            new() { Id = "tax-units", TitleKey = "Master_TaxUnits", Icon = "bi-percent", CountBadge = counts.TaxUnitsCount.ToString(), RequiredPermission = Permissions.Settings.View, PrimaryActionKey = "Master_AddTaxUnit" },
            new() { Id = "payment-modes", TitleKey = "Master_PaymentModes", Icon = "bi-credit-card", CountBadge = counts.PaymentModesCount.ToString(), RequiredPermission = Permissions.Billing.View, PrimaryActionKey = "Master_AddPaymentMode" },
            new() { Id = "expense-heads", TitleKey = "Master_ExpenseHeads", Icon = "bi-cash-stack", CountBadge = counts.ExpenseHeadsCount.ToString(), RequiredPermission = Permissions.Accounts.View, FeatureKey = FeatureKeys.Accounts, PrimaryActionKey = "Master_AddExpenseHead" },
            new() { Id = "hotel-info", TitleKey = "Master_HotelInfo", Icon = "bi-gear", CountBadge = counts.HotelInfoBadge, RequiredPermission = Permissions.Settings.View, PrimaryActionKey = "Master_EditInfo" }
        };

        var visibleTabs = new List<MasterTabDefinition>();
        foreach (var tabDef in allTabs)
        {
            if (await IsTabAllowedAsync(tabDef.RequiredPermission, tabDef.FeatureKey))
            {
                visibleTabs.Add(tabDef);
            }
        }

        var activeTabId = !string.IsNullOrWhiteSpace(requestedTab)
            ? requestedTab.Trim().ToLowerInvariant()
            : (Request.Cookies.TryGetValue("masters_active_tab", out var savedTab) && !string.IsNullOrEmpty(savedTab) ? savedTab : "menu-items");

        if (!visibleTabs.Any(t => t.Id.Equals(activeTabId, StringComparison.OrdinalIgnoreCase)))
        {
            activeTabId = visibleTabs.FirstOrDefault()?.Id ?? "menu-items";
        }

        Response.Cookies.Append("masters_active_tab", activeTabId, new Microsoft.AspNetCore.Http.CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            HttpOnly = false,
            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
        });

        var model = new MastersViewModel
        {
            ActiveTab = activeTabId,
            SelectedCategory = category ?? "All",
            Counts = counts,
            Tabs = visibleTabs,
            MenuItems = activeTabId == "menu-items" ? await _mastersUiData.GetMenuItemsAsync(category, cancellationToken) : null,
            Categories = (activeTabId == "categories" || activeTabId == "menu-items") ? await _mastersUiData.GetCategoriesAsync(cancellationToken) : null,
            Floors = activeTabId == "floors" ? await _mastersUiData.GetFloorsAsync(cancellationToken) : null,
            Tables = activeTabId == "tables" ? await _mastersUiData.GetTablesAsync(cancellationToken) : null,
            Customers = activeTabId == "customers" ? await _mastersUiData.GetCustomersAsync(cancellationToken) : null,
            Suppliers = activeTabId == "suppliers" ? await _mastersUiData.GetSuppliersAsync(cancellationToken) : null,
            Designations = activeTabId == "designations" ? await _mastersUiData.GetDesignationsAsync(cancellationToken) : null,
            Shifts = activeTabId == "shifts" ? await _mastersUiData.GetShiftsAsync(cancellationToken) : null,
            TaxUnits = activeTabId == "tax-units" ? await _mastersUiData.GetTaxUnitsAsync(cancellationToken) : null,
            PaymentModes = activeTabId == "payment-modes" ? await _mastersUiData.GetPaymentModesAsync(cancellationToken) : null,
            ExpenseHeads = activeTabId == "expense-heads" ? await _mastersUiData.GetExpenseHeadsAsync(cancellationToken) : null,
            HotelInfo = activeTabId == "hotel-info" ? await _mastersUiData.GetHotelInfoAsync(cancellationToken) : null
        };

        return model;
    }
}
