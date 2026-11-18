namespace RestaurantMS.Application.Common.Models.UiData;

public record MastersTabCountsDto(
    int FloorsCount,
    int TablesCount,
    int CategoriesCount,
    int MenuItemsCount,
    int CustomersCount,
    int SuppliersCount,
    int DesignationsCount,
    int ShiftsCount,
    int TaxUnitsCount,
    int PaymentModesCount,
    int ExpenseHeadsCount,
    string HotelInfoBadge);
