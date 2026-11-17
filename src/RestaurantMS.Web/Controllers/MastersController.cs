using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class MastersController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var masters = new List<MasterItemModel>
        {
            // Menu & Catalog
            new()
            {
                Title = "Menu Categories",
                Subtitle = "Manage food and beverage categories, ordering, and grouping",
                Controller = "Menu",
                Action = "Categories",
                Icon = "bi-card-list",
                ColorHex = "#3D1F6D",
                BadgeText = "8 Categories",
                Category = "Menu & Catalog",
                RequiredPermission = Permissions.Menu.View
            },
            new()
            {
                Title = "Dishes & Items",
                Subtitle = "Manage recipe dishes, veg/non-veg tags, pricing, and images",
                Controller = "Menu",
                Action = "Items",
                Icon = "bi-egg-fried",
                ColorHex = "#FF9F1C",
                BadgeText = "48 Dishes",
                Category = "Menu & Catalog",
                RequiredPermission = Permissions.Menu.View
            },

            // Floor & Seating
            new()
            {
                Title = "Tables & Sections",
                Subtitle = "Configure dining sections, tables layout, and capacities",
                Controller = "Tables",
                Action = "Index",
                Icon = "bi-grid-3x3-gap",
                ColorHex = "#0B7A70",
                BadgeText = "16 Tables",
                Category = "Floor & Seating",
                RequiredPermission = Permissions.Tables.View
            },
            new()
            {
                Title = "Table Reservations",
                Subtitle = "Manage advance guest reservations and pre-assigned tables",
                Controller = "Reservations",
                Action = "Index",
                Icon = "bi-calendar-check",
                ColorHex = "#3D1F6D",
                BadgeText = "Active Bookings",
                Category = "Floor & Seating",
                RequiredPermission = Permissions.Reservations.View
            },

            // CRM & Relations
            new()
            {
                Title = "Customers & Credit",
                Subtitle = "Customer directory, udhari ledger accounts, and credit limits",
                Controller = "Customers",
                Action = "Index",
                Icon = "bi-people",
                ColorHex = "#C62839",
                BadgeText = "Credit Ledger",
                Category = "CRM & Relations",
                RequiredPermission = Permissions.Customers.View
            },
            new()
            {
                Title = "Staff & Employees",
                Subtitle = "Manage waiters, kitchen staff, managers, and shifts",
                Controller = "Staff",
                Action = "Index",
                Icon = "bi-person-badge",
                ColorHex = "#0B7A70",
                BadgeText = "12 Staff",
                Category = "CRM & Relations",
                RequiredPermission = Permissions.Staff.View
            },
            new()
            {
                Title = "Staff Attendance",
                Subtitle = "Daily staff attendance logging, punch timings, and leave tracking",
                Controller = "Staff",
                Action = "Attendance",
                Icon = "bi-calendar2-check",
                ColorHex = "#0B7A70",
                BadgeText = "Daily Log",
                Category = "CRM & Relations",
                RequiredPermission = Permissions.Staff.View
            },
            new()
            {
                Title = "Employee Advances",
                Subtitle = "Advance salary disbursement, repayment records, and loan ledgers",
                Controller = "Staff",
                Action = "Advances",
                Icon = "bi-cash-coin",
                ColorHex = "#C62839",
                BadgeText = "Advances",
                Category = "CRM & Relations",
                RequiredPermission = Permissions.Staff.View
            },
            new()
            {
                Title = "Salary Statement",
                Subtitle = "Monthly payroll computation, overtime bonus, and net payout slips",
                Controller = "Staff",
                Action = "SalaryReport",
                Icon = "bi-file-earmark-spreadsheet",
                ColorHex = "#3D1F6D",
                BadgeText = "Payroll",
                Category = "CRM & Relations",
                RequiredPermission = Permissions.Staff.View
            },
            new()
            {
                Title = "Employee Performance",
                Subtitle = "Staff ratings, shifts worked, tables served, and sales contribution",
                Controller = "Staff",
                Action = "EmployeeReport",
                Icon = "bi-graph-up-arrow",
                ColorHex = "#FF9F1C",
                BadgeText = "KPIs",
                Category = "CRM & Relations",
                RequiredPermission = Permissions.Staff.View
            },

            // Supply & Inventory
            new()
            {
                Title = "Raw Materials & Stock",
                Subtitle = "Stock catalog, unit definitions, and low inventory reorder levels",
                Controller = "Inventory",
                Action = "Index",
                Icon = "bi-boxes",
                ColorHex = "#FF9F1C",
                BadgeText = "24 Items",
                Category = "Supply & Inventory",
                RequiredPermission = Permissions.Inventory.View
            },
            new()
            {
                Title = "Suppliers & Vendors",
                Subtitle = "Vendor contacts, procurement terms, and supplier ledger",
                Controller = "Inventory",
                Action = "Suppliers",
                Icon = "bi-truck",
                ColorHex = "#3D1F6D",
                BadgeText = "Vendors",
                Category = "Supply & Inventory",
                RequiredPermission = Permissions.Inventory.View
            },

            // Accounts & Finance
            new()
            {
                Title = "Chart of Accounts",
                Subtitle = "General ledger tree, assets, liabilities, income, and expenses",
                Controller = "Accounts",
                Action = "Index",
                Icon = "bi-journal-bookmark",
                ColorHex = "#0B7A70",
                BadgeText = "Ledgers",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Accounting Vouchers",
                Subtitle = "Receipts, payments, journal vouchers, and ledger postings",
                Controller = "Accounts",
                Action = "Vouchers",
                Icon = "bi-receipt",
                ColorHex = "#C62839",
                BadgeText = "Vouchers",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Day Book",
                Subtitle = "Daily debit and credit transaction chronology with single date picker",
                Controller = "Accounts",
                Action = "DayBook",
                Icon = "bi-calendar-day",
                ColorHex = "#3D1F6D",
                BadgeText = "Daily Book",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Cash Book",
                Subtitle = "Cash inflows, outflows, daily opening and closing register",
                Controller = "Accounts",
                Action = "CashBook",
                Icon = "bi-cash-stack",
                ColorHex = "#0B7A70",
                BadgeText = "Cash Flow",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Bank Book",
                Subtitle = "Bank deposits, UPI receipts, card settlements, and bank reconciliation",
                Controller = "Accounts",
                Action = "BankBook",
                Icon = "bi-bank",
                ColorHex = "#FF9F1C",
                BadgeText = "Bank Book",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Trial Balance",
                Subtitle = "Ledger balances debit vs credit verification statement as of date",
                Controller = "Accounts",
                Action = "TrialBalance",
                Icon = "bi-scale",
                ColorHex = "#3D1F6D",
                BadgeText = "Trial Balance",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Profit & Loss Statement",
                Subtitle = "Gross turnover, COGS direct cost, operating expenses, and net profit",
                Controller = "Accounts",
                Action = "ProfitLoss",
                Icon = "bi-graph-up",
                ColorHex = "#0B7A70",
                BadgeText = "P & L",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Balance Sheet",
                Subtitle = "Financial position statement: Current & Fixed Assets vs Liabilities & Equity",
                Controller = "Accounts",
                Action = "BalanceSheet",
                Icon = "bi-bar-chart-steps",
                ColorHex = "#C62839",
                BadgeText = "Balance Sheet",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Ledger Summary",
                Subtitle = "Account-wise ledger summary, debit and credit turnover with date range",
                Controller = "Accounts",
                Action = "LedgerSummary",
                Icon = "bi-journals",
                ColorHex = "#FF9F1C",
                BadgeText = "Ledger Head",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },
            new()
            {
                Title = "Accounts Audit Report",
                Subtitle = "Statutory double-entry audit verification, voucher validation, and ledger check",
                Controller = "Accounts",
                Action = "AuditReport",
                Icon = "bi-shield-check",
                ColorHex = "#3D1F6D",
                BadgeText = "Statutory Audit",
                Category = "Accounts & Finance",
                RequiredPermission = Permissions.Accounts.View
            },

            // System & Configuration
            new()
            {
                Title = "System Audit & Activity Log",
                Subtitle = "Who generated what: bill settlement, KOT, voids, discounts, logins (Admin only)",
                Controller = "AuditLogs",
                Action = "Index",
                Icon = "bi-activity",
                ColorHex = "#C62839",
                BadgeText = "Admin Only",
                Category = "System Configuration",
                RequiredPermission = Permissions.Settings.View
            },

            // System & Configuration
            new()
            {
                Title = "Hotel Profile & Settings",
                Subtitle = "Hotel name, GSTIN, FSSAI, logo, billing, and receipt headers",
                Controller = "Settings",
                Action = "Index",
                Icon = "bi-gear",
                ColorHex = "#3D1F6D",
                BadgeText = "System Config",
                Category = "System Configuration",
                RequiredPermission = Permissions.Settings.View
            },
            new()
            {
                Title = "System Users",
                Subtitle = "User accounts, logins, password reset, and access status",
                Controller = "Users",
                Action = "Index",
                Icon = "bi-person-gear",
                ColorHex = "#0B7A70",
                BadgeText = "Users",
                Category = "System Configuration",
                RequiredPermission = Permissions.Users.View
            },
            new()
            {
                Title = "Roles & Permissions",
                Subtitle = "Role definitions and granular screen-level permission matrix",
                Controller = "Roles",
                Action = "Index",
                Icon = "bi-shield-lock",
                ColorHex = "#FF9F1C",
                BadgeText = "Security Matrix",
                Category = "System Configuration",
                RequiredPermission = Permissions.Roles.View
            },
            new()
            {
                Title = "Feature Toggles",
                Subtitle = "Enable or disable modules (KDS, Credit, Tables, Images, Accounts)",
                Controller = "FeatureToggles",
                Action = "Index",
                Icon = "bi-toggle-on",
                ColorHex = "#C62839",
                BadgeText = "Modules",
                Category = "System Configuration",
                RequiredPermission = Permissions.FeatureToggles.View
            }
        };

        var model = new MastersViewModel { Masters = masters };
        return View(model);
    }
}
