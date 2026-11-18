using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleMastersUiData : IMastersUiData
{
    private readonly List<MasterMenuItemDto> MenuItems =
    [
        new(7, "Gulab Jamun (2 Pcs)", "Warm milk dumplings in rose flavored syrup", "Desserts", "Veg", 90m, 5, "Available"),
        new(6, "Cold Coffee with Ice Cream", "Rich blended espresso with vanilla scoop", "Beverages", "Veg", 140m, 10, "Available"),
        new(5, "Cold Drink / Soda", "Chilled carbonated beverage 300ml", "Beverages", "Veg", 60m, 2, "Available"),
        new(4, "Chicken Biryani", "Flavorful Basmati rice cooked with tender chicken", "Biryani & Rice", "Non-Veg", 320m, 25, "Available"),
        new(3, "Butter Naan", "Freshly baked clay oven bread with butter", "Main Course", "Veg", 50m, 8, "Available"),
        new(2, "Dal Makhani", "Slow cooked black lentils with cream and spices", "Main Course", "Veg", 250m, 20, "Available"),
        new(1, "Paneer Tikka", "Marinated cottage cheese grilled in tandoor", "Starters", "Veg", 220m, 15, "Available")
    ];

    private readonly List<MasterCategoryDto> Categories =
    [
        new(1, "Starters", "स्टार्टर्स", 1, 1, "Active"),
        new(2, "Main Course", "मेन कोर्स", 2, 2, "Active"),
        new(3, "Biryani & Rice", "बिर्याणी व भात", 3, 1, "Active"),
        new(4, "Beverages", "पेये व सरबते", 4, 2, "Active"),
        new(5, "Desserts", "गोडधोड / मिष्टान्न", 5, 1, "Active")
    ];

    private readonly List<MasterFloorDto> Floors =
    [
        new(1, "Ground Floor AC Hall", "GF-AC", "Air-conditioned fine dining hall", 4, 16, "Active"),
        new(2, "First Floor Family Dining", "FF-FAM", "Spacious family dining area", 5, 20, "Active"),
        new(3, "Rooftop Terrace Garden", "RF-TER", "Open-air scenic dining ambience", 3, 12, "Active")
    ];

    private readonly List<MasterTableDto> Tables =
    [
        new(1, "T1", "Ground Floor AC Hall", 4, "Square", "Available"),
        new(2, "T2", "Ground Floor AC Hall", 4, "Square", "Available"),
        new(3, "T3", "Ground Floor AC Hall", 2, "Round", "Available"),
        new(4, "T4", "Ground Floor AC Hall", 6, "Rectangle", "Available"),
        new(5, "T5", "First Floor Family Dining", 4, "Square", "Available"),
        new(6, "T6", "First Floor Family Dining", 4, "Square", "Available"),
        new(7, "T7", "First Floor Family Dining", 6, "Rectangle", "Available"),
        new(8, "T8", "First Floor Family Dining", 4, "Square", "Available"),
        new(9, "T9", "Rooftop Terrace Garden", 4, "Round", "Available")
    ];

    private readonly List<MasterCustomerDto> Customers =
    [
        new(1, "Rahul Sharma", "9822012345", "102 Kothrud Heights, Pune", 15000m, 2450m, "Active"),
        new(2, "Pooja Patil", "9822023456", "B-4 Baner Road, Pune", 10000m, 0m, "Active"),
        new(3, "Amit Deshmukh", "9822034567", "55 Deccan Gymkhana, Pune", 20000m, 4800m, "Active"),
        new(4, "Suresh Kadam", "9822045678", "12 Hadapsar Solapur Rd, Pune", 5000m, 1200m, "Active"),
        new(5, "Kiran Shinde", "9822056789", "78 Viman Nagar, Pune", 12000m, 0m, "Active")
    ];

    private readonly List<MasterSupplierDto> Suppliers =
    [
        new(1, "Metro Cash & Carry", "Vikas Shinde", "9823011223", "27AAACM1234F1Z5", "Dairy & Grocery", "Active"),
        new(2, "Sahyadri Dairy Farm", "Santosh Jagtap", "9823022334", "27AAACS5678G2Z1", "Fresh Milk & Paneer", "Active"),
        new(3, "Konkan Sea Food & Poultry", "Mahesh Sawant", "9823033445", "27AAACK9012H3Z3", "Poultry & Non-Veg", "Active"),
        new(4, "Poonam Spices & Condiments", "Ramesh Agarwal", "9823044556", "27AAACP3456J4Z7", "Spices & Oils", "Active")
    ];

    private readonly List<MasterDesignationDto> Designations =
    [
        new(1, "Head Chef", "Kitchen", "L3", 2, "Active"),
        new(2, "Captain / Senior Waiter", "Service", "L2", 4, "Active"),
        new(3, "Cashier", "Accounts", "L2", 2, "Active"),
        new(4, "Kitchen Helper", "Kitchen", "L1", 3, "Active"),
        new(5, "Restaurant Manager", "Management", "L4", 1, "Active")
    ];

    private readonly List<MasterShiftDto> Shifts =
    [
        new(1, "Morning Shift", "07:00 AM", "03:30 PM", 15, 6, "Active"),
        new(2, "Evening Shift", "03:30 PM", "11:30 PM", 15, 8, "Active"),
        new(3, "Full Day Shift", "10:00 AM", "10:00 PM", 30, 2, "Active")
    ];

    private readonly List<MasterTaxUnitDto> TaxUnits =
    [
        new(1, "GST 5% (Restaurant Service)", "Tax Slab", "5%", "Active"),
        new(2, "GST 18% (Beverages / Events)", "Tax Slab", "18%", "Active"),
        new(3, "GST 0% (Fresh Farm Items)", "Tax Slab", "0%", "Active"),
        new(4, "Kilogram (Kg)", "Measurement Unit", "kg", "Active"),
        new(5, "Liter (L)", "Measurement Unit", "L", "Active"),
        new(6, "Plate / Portion", "Measurement Unit", "portion", "Active")
    ];

    private readonly List<MasterPaymentModeDto> PaymentModes =
    [
        new(1, "Cash", "CASH", 0m, "Active"),
        new(2, "UPI (PhonePe, GPay, Paytm)", "UPI", 0m, "Active"),
        new(3, "Credit / Debit Card", "CARD", 1.5m, "Active"),
        new(4, "Udhari (Customer Credit Ledger)", "UDHARI", 0m, "Active"),
        new(5, "Net Banking / NEFT", "NETBANK", 0m, "Active")
    ];

    private readonly List<MasterExpenseHeadDto> ExpenseHeads =
    [
        new(1, "Kitchen Raw Materials & Grocery", "Operating", 150000m, true, "Active"),
        new(2, "Utilities & Electricity Bill", "Facility", 35000m, true, "Active"),
        new(3, "Premises Rent", "Facility", 80000m, true, "Active"),
        new(4, "Staff Salaries & Wages", "Payroll", 120000m, true, "Active"),
        new(5, "Kitchen Equipment Maintenance", "Maintenance", 15000m, true, "Active"),
        new(6, "Marketing & Promotional Expenses", "Operating", 20000m, true, "Active")
    ];

    private static readonly MasterHotelInfoDto HotelInfo = new(
        "Grand Restaurant",
        "भव्य रेस्टॉरंट",
        "27AAAAA0000A1Z2",
        "12345678901234",
        "123 Main Street, Pune, MH, India",
        "9876543210",
        "info@grandrestaurant.com",
        "INR (₹)",
        "Asia/Kolkata",
        "Thank you for dining with us!",
        "Visit Again! Powered by RestaurantMS"
    );

    public Task<MastersTabCountsDto> GetTabCountsAsync(CancellationToken cancellationToken = default)
    {
        var counts = new MastersTabCountsDto(
            Floors.Count,
            Tables.Count,
            Categories.Count,
            MenuItems.Count,
            Customers.Count,
            Suppliers.Count,
            Designations.Count,
            Shifts.Count,
            TaxUnits.Count,
            PaymentModes.Count,
            ExpenseHeads.Count,
            "GST & Info"
        );
        return Task.FromResult(counts);
    }

    public Task<IReadOnlyList<MasterMenuItemDto>> GetMenuItemsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category) || category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IReadOnlyList<MasterMenuItemDto>>(MenuItems);
        }

        var filtered = MenuItems.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IReadOnlyList<MasterMenuItemDto>>(filtered);
    }

    public Task<IReadOnlyList<MasterCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterCategoryDto>>(Categories);
    }

    public Task<IReadOnlyList<MasterFloorDto>> GetFloorsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterFloorDto>>(Floors);
    }

    public Task<IReadOnlyList<MasterTableDto>> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterTableDto>>(Tables);
    }

    public Task<IReadOnlyList<MasterCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterCustomerDto>>(Customers);
    }

    public Task<IReadOnlyList<MasterSupplierDto>> GetSuppliersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterSupplierDto>>(Suppliers);
    }

    public Task<IReadOnlyList<MasterDesignationDto>> GetDesignationsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterDesignationDto>>(Designations);
    }

    public Task<IReadOnlyList<MasterShiftDto>> GetShiftsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterShiftDto>>(Shifts);
    }

    public Task<IReadOnlyList<MasterTaxUnitDto>> GetTaxUnitsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterTaxUnitDto>>(TaxUnits);
    }

    public Task<IReadOnlyList<MasterPaymentModeDto>> GetPaymentModesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterPaymentModeDto>>(PaymentModes);
    }

    public Task<IReadOnlyList<MasterExpenseHeadDto>> GetExpenseHeadsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<MasterExpenseHeadDto>>(ExpenseHeads);
    }

    public Task<MasterHotelInfoDto> GetHotelInfoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HotelInfo);
    }

    public Task<string> SaveEntryAsync(string tab, IReadOnlyDictionary<string, string> fields, CancellationToken cancellationToken = default)
    {
        string Get(string key, string fallback = "") =>
            fields.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val) ? val.Trim() : fallback;

        decimal GetDec(string key, decimal fallback = 0m) =>
            decimal.TryParse(Get(key), out var d) ? d : fallback;

        int GetInt(string key, int fallback = 0) =>
            int.TryParse(Get(key), out var i) ? i : fallback;

        var isActive = !string.Equals(Get("IsActive", "true"), "false", StringComparison.OrdinalIgnoreCase);
        var status = isActive ? "Active" : "Inactive";
        var keyNormalized = (tab ?? string.Empty).Trim().ToLowerInvariant();
        string highlight = string.Empty;

        switch (keyNormalized)
        {
            case "floors":
                highlight = Get("Name", "New Floor");
                Floors.Insert(0, new MasterFloorDto(Floors.Count + 1, highlight, Get("Code", $"FL-{Floors.Count + 1}"), Get("Description", "Dining floor"), GetInt("TableCount", 4), GetInt("SeatingCapacity", 16), status));
                break;
            case "tables":
                highlight = Get("TableCode", $"T{Tables.Count + 1}");
                Tables.Insert(0, new MasterTableDto(Tables.Count + 1, highlight, Get("FloorName", "Ground Floor AC Hall"), GetInt("Capacity", 4), Get("Shape", "Square"), isActive ? "Available" : "Inactive"));
                break;
            case "categories":
                highlight = Get("NameEn", Get("Name", "New Category"));
                Categories.Insert(0, new MasterCategoryDto(Categories.Count + 1, highlight, Get("NameMr", highlight), GetInt("DisplayOrder", Categories.Count + 1), 0, status));
                break;
            case "menu-items":
                highlight = Get("NameEn", Get("Name", "New Dish"));
                MenuItems.Insert(0, new MasterMenuItemDto(MenuItems.Count + 1, highlight, Get("Description", "Freshly prepared dish"), Get("Category", "Main Course"), Get("DietaryType", "Veg"), GetDec("Price", 150m), GetInt("PrepTimeMinutes", 15), isActive ? "Available" : "Unavailable"));
                break;
            case "customers":
                highlight = Get("FullName", Get("Name", "New Customer"));
                Customers.Insert(0, new MasterCustomerDto(Customers.Count + 1, highlight, Get("MobileNumber", "9822000000"), Get("Address", "Pune"), GetDec("CreditLimit", 5000m), 0m, status));
                break;
            case "suppliers":
                highlight = Get("Name", "New Supplier");
                Suppliers.Insert(0, new MasterSupplierDto(Suppliers.Count + 1, highlight, Get("ContactPerson", "Manager"), Get("Phone", "9823000000"), Get("Gstin", "-"), Get("Category", "Grocery"), status));
                break;
            case "designations":
                highlight = Get("Title", "New Role");
                Designations.Insert(0, new MasterDesignationDto(Designations.Count + 1, highlight, Get("Department", "Service"), Get("Level", "L2"), GetInt("Headcount", 1), status));
                break;
            case "shifts":
                highlight = Get("Name", "New Shift");
                Shifts.Insert(0, new MasterShiftDto(Shifts.Count + 1, highlight, Get("StartTime", "09:00 AM"), Get("EndTime", "06:00 PM"), GetInt("GracePeriodMinutes", 15), 0, status));
                break;
            case "tax-units":
                highlight = Get("Name", "New Tax / Unit");
                TaxUnits.Insert(0, new MasterTaxUnitDto(TaxUnits.Count + 1, highlight, Get("Type", "Tax Slab"), Get("RateOrSymbol", "5%"), status));
                break;
            case "payment-modes":
                highlight = Get("Name", "New Mode");
                PaymentModes.Insert(0, new MasterPaymentModeDto(PaymentModes.Count + 1, highlight, Get("Code", "MODE").ToUpperInvariant(), GetDec("FeePercent", 0m), status));
                break;
            case "expense-heads":
                highlight = Get("Name", "New Expense Head");
                ExpenseHeads.Insert(0, new MasterExpenseHeadDto(ExpenseHeads.Count + 1, highlight, Get("Category", "Operating"), GetDec("MonthlyBudget", 10000m), !string.Equals(Get("IsTaxDeductible", "true"), "false", StringComparison.OrdinalIgnoreCase), status));
                break;
        }

        return Task.FromResult(highlight);
    }
}
