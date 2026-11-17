using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasherService passwordHasher)
    {
        // 1. Seed Permissions
        var allPermissionCodes = Permissions.GetAll();
        var existingPerms = await context.Permissions.ToListAsync();
        var existingCodes = existingPerms.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newPermissions = new List<Permission>();
        foreach (var code in allPermissionCodes)
        {
            if (!existingCodes.Contains(code))
            {
                var parts = code.Split('.');
                var screen = parts[0];
                var action = parts.Length > 1 ? parts[1] : "View";

                var perm = new Permission
                {
                    Code = code,
                    Screen = screen,
                    Action = action,
                    Description = $"{action} permission for {screen} screen",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                newPermissions.Add(perm);
                existingPerms.Add(perm);
            }
        }

        if (newPermissions.Count > 0)
        {
            await context.Permissions.AddRangeAsync(newPermissions);
            await context.SaveChangesAsync();
        }

        // 2. Seed Roles
        var roleDefinitions = new (string Name, string Description)[]
        {
            ("Owner", "Full system ownership and control"),
            ("Manager", "Day-to-day operations and staff oversight"),
            ("Cashier", "Billing and payment processing"),
            ("Waiter", "Table service and order taking"),
            ("Kitchen", "Kitchen display and order preparation"),
            ("Accountant", "Financial management and bookkeeping")
        };

        var existingRoles = await context.Roles.Include(r => r.RolePermissions).ToListAsync();
        var rolesByName = existingRoles.ToDictionary(r => r.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var def in roleDefinitions)
        {
            if (!rolesByName.ContainsKey(def.Name))
            {
                var role = new Role
                {
                    Name = def.Name,
                    Description = def.Description,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Roles.Add(role);
                rolesByName[def.Name] = role;
            }
        }

        await context.SaveChangesAsync();

        // 3. Seed Role Permissions Mapping
        var permsByCode = (await context.Permissions.ToListAsync())
            .ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        var existingRolePerms = await context.RolePermissions.ToListAsync();
        var existingRolePermKeys = existingRolePerms
            .Select(rp => $"{rp.RoleId}_{rp.PermissionId}")
            .ToHashSet();

        var newRolePermissions = new List<RolePermission>();

        void Assign(string roleName, IEnumerable<string> codes)
        {
            if (!rolesByName.TryGetValue(roleName, out var role)) return;

            foreach (var code in codes)
            {
                if (permsByCode.TryGetValue(code, out var perm))
                {
                    var key = $"{role.Id}_{perm.Id}";
                    if (!existingRolePermKeys.Contains(key))
                    {
                        newRolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = perm.Id,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System"
                        });
                        existingRolePermKeys.Add(key);
                    }
                }
            }
        }

        // Owner: All
        Assign("Owner", allPermissionCodes);

        // Manager: All except Settings.Edit and FeatureToggles.Edit
        Assign("Manager", allPermissionCodes.Where(c => c != Permissions.Settings.Edit && c != Permissions.FeatureToggles.Edit));

        // Cashier
        Assign("Cashier",
        [
            Permissions.Dashboard.View,
            Permissions.Tables.View,
            Permissions.Parcel.View, Permissions.Parcel.Create, Permissions.Parcel.Edit, Permissions.Parcel.Print,
            Permissions.Orders.View, Permissions.Orders.Create, Permissions.Orders.Edit, Permissions.Orders.Print,
            Permissions.Billing.View, Permissions.Billing.Create, Permissions.Billing.Edit, Permissions.Billing.Print,
            Permissions.Customers.View, Permissions.Customers.Create, Permissions.Customers.Edit,
            Permissions.Credit.View, Permissions.Credit.Create, Permissions.Credit.Edit,
            Permissions.DayClosing.View, Permissions.DayClosing.Create, Permissions.DayClosing.Print
        ]);

        // Waiter
        Assign("Waiter",
        [
            Permissions.Dashboard.View,
            Permissions.Tables.View,
            Permissions.Parcel.View, Permissions.Parcel.Create,
            Permissions.Orders.View, Permissions.Orders.Create, Permissions.Orders.Print
        ]);

        // Kitchen
        Assign("Kitchen",
        [
            Permissions.Kitchen.View, Permissions.Kitchen.Edit, Permissions.Kitchen.Print,
            Permissions.Orders.View
        ]);

        // Accountant
        Assign("Accountant",
        [
            Permissions.Dashboard.View, Permissions.Dashboard.Export,
            Permissions.Billing.View, Permissions.Billing.Export,
            Permissions.Credit.View, Permissions.Credit.Export,
            Permissions.Accounts.View, Permissions.Accounts.Create, Permissions.Accounts.Edit, Permissions.Accounts.Print, Permissions.Accounts.Export,
            Permissions.Reports.View, Permissions.Reports.Print, Permissions.Reports.Export
        ]);

        if (newRolePermissions.Count > 0)
        {
            await context.RolePermissions.AddRangeAsync(newRolePermissions);
            await context.SaveChangesAsync();
        }

        // 4. Seed Settings
        var defaultSettings = new (string Key, string Value, SettingDataType Type, string Group)[]
        {
            ("HotelNameEn", "Grand Restaurant", SettingDataType.String, "Profile"),
            ("HotelNameMr", "भव्य रेस्टॉरंट", SettingDataType.String, "Profile"),
            ("LogoUrl", "/images/logo.png", SettingDataType.String, "Profile"),
            ("Address", "123 Main Street, Pune, MH, India", SettingDataType.String, "Profile"),
            ("ContactNumber", "9876543210", SettingDataType.String, "Profile"),
            ("Email", "info@grandrestaurant.com", SettingDataType.String, "Profile"),
            ("Gstin", "27AAAAA0000A1Z2", SettingDataType.String, "Billing"),
            ("Fssai", "12345678901234", SettingDataType.String, "Billing"),
            ("Currency", "INR", SettingDataType.String, "Billing"),
            ("Timezone", "Asia/Kolkata", SettingDataType.String, "General"),
            ("ReceiptHeader", "Thank you for dining with us!", SettingDataType.String, "Printing"),
            ("ReceiptFooter", "Visit Again! Powered by RestaurantMS", SettingDataType.String, "Printing")
        };

        var existingSettings = await context.HotelSettings.ToListAsync();
        var existingSettingKeys = existingSettings.Select(s => s.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newSettings = new List<HotelSetting>();
        foreach (var (key, value, type, group) in defaultSettings)
        {
            if (!existingSettingKeys.Contains(key))
            {
                newSettings.Add(new HotelSetting
                {
                    Key = key,
                    Value = value,
                    DataType = type,
                    Group = group,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }
        }

        if (newSettings.Count > 0)
        {
            await context.HotelSettings.AddRangeAsync(newSettings);
            await context.SaveChangesAsync();
        }

        // 5. Seed Feature Toggles (all toggles ON)
        var toggleDefinitions = new (string Key, string Name, string Description, string Group)[]
        {
            (FeatureKeys.KitchenDisplay, "Kitchen Display System (KDS)", "Digital order display and management in the kitchen", "Operations"),
            (FeatureKeys.Kitchen, "Kitchen Module", "Enable or disable the kitchen display screen", "Operations"),
            (FeatureKeys.InventoryTracking, "Inventory & Stock Tracking", "Real-time stock deduction, ingredient mapping and alerts", "Operations"),
            (FeatureKeys.CreditUdhari, "Credit (Udhari) Management", "Customer credit accounts, ledger tracking and settlements", "Billing"),
            (FeatureKeys.Credit, "Credit & Udhari Options", "Show or hide credit options in customer and billing screens", "Billing"),
            (FeatureKeys.CustomerLoyalty, "Customer Loyalty Program", "Reward points, membership tiers and repeat customer discounts", "Marketing"),
            (FeatureKeys.OnlineOrdering, "Online Ordering Integration", "Accept orders from Swiggy, Zomato and direct web store", "Integrations"),
            (FeatureKeys.TableReservation, "Table Reservation", "Advance table booking, guest preferences and slot allocations", "Operations"),
            (FeatureKeys.Tables, "Tables Module", "Enable or disable dine-in tables screen", "Operations"),
            (FeatureKeys.Accounts, "Accounts Module", "Enable or disable accounting screens and charts of accounts", "Enterprise"),
            (FeatureKeys.Images, "Menu Images", "Show or hide item photos in menu and order screens", "Marketing"),
            (FeatureKeys.MultiCurrency, "Multi-Currency Billing", "Support for foreign currency billing and exchange conversions", "Billing"),
            (FeatureKeys.BarcodeScanning, "Barcode Scanning", "Scan barcodes for fast item addition and inventory counts", "Operations"),
            (FeatureKeys.SmsNotifications, "SMS Notifications", "Automated SMS updates for bills, OTP and reservations", "Integrations"),
            (FeatureKeys.EmailReceipts, "Email Receipts", "Send PDF digital receipts directly to customer email", "Integrations"),
            (FeatureKeys.MultiBranch, "Multi-Branch Management", "Centralized oversight across multiple hotel branches", "Enterprise"),
            (FeatureKeys.PrintKot, "Kitchen Order Ticket (KOT) Printing", "Direct thermal printing of KOTs in the kitchen", "Printing"),
            (FeatureKeys.RoundOffBilling, "Round-Off Bill Totals", "Automatically round final invoice totals to the nearest rupee", "Billing"),
            (FeatureKeys.SplitBilling, "Split Billing", "Allow parties to divide bills equally or item-wise", "Billing"),
            (FeatureKeys.ServiceCharges, "Service Charge", "Optionally apply standard discretionary restaurant service charge", "Billing")
        };

        var existingToggles = await context.FeatureToggles.ToListAsync();
        var existingToggleKeys = existingToggles.Select(t => t.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newToggles = new List<FeatureToggle>();
        foreach (var (key, name, desc, group) in toggleDefinitions)
        {
            if (!existingToggleKeys.Contains(key))
            {
                newToggles.Add(new FeatureToggle
                {
                    Key = key,
                    Name = name,
                    Description = desc,
                    Group = group,
                    IsEnabled = true, // all feature toggles ON
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }
        }

        if (newToggles.Count > 0)
        {
            await context.FeatureToggles.AddRangeAsync(newToggles);
            await context.SaveChangesAsync();
        }

        // 6. Seed Admin User
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserId == "admin");
        if (adminUser is null)
        {
            var ownerRole = rolesByName["Owner"];
            adminUser = new AppUser
            {
                UserId = "admin",
                FullName = "System Administrator",
                Email = "admin@restaurantms.local",
                PhoneNumber = "9999999999",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                RoleId = ownerRole.Id,
                IsActive = true,
                MustChangePassword = true, // forced password change at first login
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}

