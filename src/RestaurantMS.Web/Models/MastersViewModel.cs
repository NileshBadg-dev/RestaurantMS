using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class MastersViewModel
{
    public string ActiveTab { get; init; } = "menu-items";
    public string? SelectedCategory { get; init; } = "All";
    public MastersTabCountsDto Counts { get; init; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "GST & Info");
    public IReadOnlyList<MasterTabDefinition> Tabs { get; init; } = new List<MasterTabDefinition>();

    // Tab Specific Loaded Data
    public IReadOnlyList<MasterMenuItemDto>? MenuItems { get; init; }
    public IReadOnlyList<MasterCategoryDto>? Categories { get; init; }
    public IReadOnlyList<MasterFloorDto>? Floors { get; init; }
    public IReadOnlyList<MasterTableDto>? Tables { get; init; }
    public IReadOnlyList<MasterCustomerDto>? Customers { get; init; }
    public IReadOnlyList<MasterSupplierDto>? Suppliers { get; init; }
    public IReadOnlyList<MasterDesignationDto>? Designations { get; init; }
    public IReadOnlyList<MasterShiftDto>? Shifts { get; init; }
    public IReadOnlyList<MasterTaxUnitDto>? TaxUnits { get; init; }
    public IReadOnlyList<MasterPaymentModeDto>? PaymentModes { get; init; }
    public IReadOnlyList<MasterExpenseHeadDto>? ExpenseHeads { get; init; }
    public MasterHotelInfoDto? HotelInfo { get; init; }
}
