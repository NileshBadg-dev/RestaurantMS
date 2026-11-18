using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IMastersUiData
{
    Task<MastersTabCountsDto> GetTabCountsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterMenuItemDto>> GetMenuItemsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterFloorDto>> GetFloorsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterTableDto>> GetTablesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterSupplierDto>> GetSuppliersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterDesignationDto>> GetDesignationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterShiftDto>> GetShiftsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterTaxUnitDto>> GetTaxUnitsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterPaymentModeDto>> GetPaymentModesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterExpenseHeadDto>> GetExpenseHeadsAsync(CancellationToken cancellationToken = default);
    Task<MasterHotelInfoDto> GetHotelInfoAsync(CancellationToken cancellationToken = default);
    Task<string> SaveEntryAsync(string tab, IReadOnlyDictionary<string, string> fields, CancellationToken cancellationToken = default);
}
