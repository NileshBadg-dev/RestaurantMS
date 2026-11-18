namespace RestaurantMS.Domain.Constants;

public static class Permissions
{
    public static class Screens
    {
        public const string Dashboard = "Dashboard";
        public const string Users = "Users";
        public const string Roles = "Roles";
        public const string Settings = "Settings";
        public const string FeatureToggles = "FeatureToggles";
        public const string AuditLogs = "AuditLogs";
        public const string Tables = "Tables";
        public const string Parcel = "Parcel";
        public const string Orders = "Orders";
        public const string Kitchen = "Kitchen";
        public const string Billing = "Billing";
        public const string Customers = "Customers";
        public const string Credit = "Credit";
        public const string Accounts = "Accounts";
        public const string Menu = "Menu";
        public const string Inventory = "Inventory";
        public const string Reports = "Reports";
        public const string Reservations = "Reservations";
        public const string Staff = "Staff";
        public const string EmployeeAdvances = "EmployeeAdvances";
        public const string DayClosing = "DayClosing";
        public const string OnlineOrders = "OnlineOrders";
        public const string Masters = "Masters";
    }

    public static class Actions
    {
        public const string View = "View";
        public const string Create = "Create";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string Print = "Print";
        public const string Export = "Export";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
        public const string Export = "Dashboard.Export";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
        public const string Export = "Users.Export";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Edit = "Roles.Edit";
        public const string Delete = "Roles.Delete";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Edit = "Settings.Edit";
    }

    public static class FeatureToggles
    {
        public const string View = "FeatureToggles.View";
        public const string Edit = "FeatureToggles.Edit";
    }

    public static class AuditLogs
    {
        public const string View = "AuditLogs.View";
        public const string Export = "AuditLogs.Export";
    }

    public static class Tables
    {
        public const string View = "Tables.View";
        public const string Create = "Tables.Create";
        public const string Edit = "Tables.Edit";
        public const string Delete = "Tables.Delete";
    }

    public static class Parcel
    {
        public const string View = "Parcel.View";
        public const string Create = "Parcel.Create";
        public const string Edit = "Parcel.Edit";
        public const string Print = "Parcel.Print";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string Create = "Orders.Create";
        public const string Edit = "Orders.Edit";
        public const string Delete = "Orders.Delete";
        public const string Print = "Orders.Print";
    }

    public static class Kitchen
    {
        public const string View = "Kitchen.View";
        public const string Edit = "Kitchen.Edit";
        public const string Print = "Kitchen.Print";
    }

    public static class Billing
    {
        public const string View = "Billing.View";
        public const string Create = "Billing.Create";
        public const string Edit = "Billing.Edit";
        public const string Print = "Billing.Print";
        public const string Export = "Billing.Export";
    }

    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Create = "Customers.Create";
        public const string Edit = "Customers.Edit";
        public const string Delete = "Customers.Delete";
        public const string Export = "Customers.Export";
    }

    public static class Credit
    {
        public const string View = "Credit.View";
        public const string Create = "Credit.Create";
        public const string Edit = "Credit.Edit";
        public const string Print = "Credit.Print";
        public const string Export = "Credit.Export";
    }

    public static class Accounts
    {
        public const string View = "Accounts.View";
        public const string Create = "Accounts.Create";
        public const string Edit = "Accounts.Edit";
        public const string Print = "Accounts.Print";
        public const string Export = "Accounts.Export";
    }

    public static class Menu
    {
        public const string View = "Menu.View";
        public const string Create = "Menu.Create";
        public const string Edit = "Menu.Edit";
        public const string Delete = "Menu.Delete";
        public const string Export = "Menu.Export";
    }

    public static class Inventory
    {
        public const string View = "Inventory.View";
        public const string Create = "Inventory.Create";
        public const string Edit = "Inventory.Edit";
        public const string Delete = "Inventory.Delete";
        public const string Purchase = "Inventory.Purchase";
        public const string Adjust = "Inventory.Adjust";
        public const string Wastage = "Inventory.Wastage";
        public const string Consumption = "Inventory.Consumption";
        public const string Export = "Inventory.Export";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Print = "Reports.Print";
        public const string Export = "Reports.Export";
        public const string Sales = "Reports.Sales";
        public const string Tables = "Reports.Tables";
        public const string Menu = "Reports.Menu";
        public const string Parcel = "Reports.Parcel";
        public const string Staff = "Reports.Staff";
        public const string Payments = "Reports.Payments";
        public const string Kitchen = "Reports.Kitchen";
        public const string Inventory = "Reports.Inventory";
        public const string Accounts = "Reports.Accounts";
        public const string Tax = "Reports.Tax";
    }

    public static class Reservations
    {
        public const string View = "Reservations.View";
        public const string Create = "Reservations.Create";
        public const string Edit = "Reservations.Edit";
        public const string Delete = "Reservations.Delete";
    }

    public static class Staff
    {
        public const string View = "Staff.View";
        public const string Create = "Staff.Create";
        public const string Edit = "Staff.Edit";
        public const string Delete = "Staff.Delete";
        public const string Attendance = "Staff.Attendance";
        public const string LockMonth = "Staff.LockMonth";
        public const string Salary = "Staff.Salary";
        public const string PrintPayslip = "Staff.PrintPayslip";
    }

    public static class EmployeeAdvances
    {
        public const string View = "EmployeeAdvances.View";
        public const string Create = "EmployeeAdvances.Create";
        public const string Recover = "EmployeeAdvances.Recover";
        public const string ApproveOverride = "EmployeeAdvances.ApproveOverride";
        public const string Export = "EmployeeAdvances.Export";
    }

    public static class DayClosing
    {
        public const string View = "DayClosing.View";
        public const string Create = "DayClosing.Create";
        public const string Print = "DayClosing.Print";
    }

    public static class OnlineOrders
    {
        public const string View = "OnlineOrders.View";
        public const string Create = "OnlineOrders.Create";
        public const string Edit = "OnlineOrders.Edit";
    }

    public static class Masters
    {
        public const string View = "Masters.View";
        public const string Create = "Masters.Create";
        public const string Edit = "Masters.Edit";
        public const string Delete = "Masters.Delete";
    }

    public static IReadOnlyList<string> GetAll()
    {
        return
        [
            Dashboard.View, Dashboard.Export,
            Users.View, Users.Create, Users.Edit, Users.Delete, Users.Export,
            Roles.View, Roles.Create, Roles.Edit, Roles.Delete,
            Settings.View, Settings.Edit,
            FeatureToggles.View, FeatureToggles.Edit,
            AuditLogs.View, AuditLogs.Export,
            Tables.View, Tables.Create, Tables.Edit, Tables.Delete,
            Parcel.View, Parcel.Create, Parcel.Edit, Parcel.Print,
            Orders.View, Orders.Create, Orders.Edit, Orders.Delete, Orders.Print,
            Kitchen.View, Kitchen.Edit, Kitchen.Print,
            Billing.View, Billing.Create, Billing.Edit, Billing.Print, Billing.Export,
            Customers.View, Customers.Create, Customers.Edit, Customers.Delete, Customers.Export,
            Credit.View, Credit.Create, Credit.Edit, Credit.Print, Credit.Export,
            Accounts.View, Accounts.Create, Accounts.Edit, Accounts.Print, Accounts.Export,
            Menu.View, Menu.Create, Menu.Edit, Menu.Delete, Menu.Export,
            Inventory.View, Inventory.Create, Inventory.Edit, Inventory.Delete, Inventory.Purchase, Inventory.Adjust, Inventory.Wastage, Inventory.Consumption, Inventory.Export,
            Reports.View, Reports.Print, Reports.Export, Reports.Sales, Reports.Tables, Reports.Menu, Reports.Parcel, Reports.Staff, Reports.Payments, Reports.Kitchen, Reports.Inventory, Reports.Accounts, Reports.Tax,
            Reservations.View, Reservations.Create, Reservations.Edit, Reservations.Delete,
            Staff.View, Staff.Create, Staff.Edit, Staff.Delete, Staff.Attendance, Staff.LockMonth, Staff.Salary, Staff.PrintPayslip,
            EmployeeAdvances.View, EmployeeAdvances.Create, EmployeeAdvances.Recover, EmployeeAdvances.ApproveOverride, EmployeeAdvances.Export,
            DayClosing.View, DayClosing.Create, DayClosing.Print,
            OnlineOrders.View, OnlineOrders.Create, OnlineOrders.Edit,
            Masters.View, Masters.Create, Masters.Edit, Masters.Delete
        ];
    }
}
