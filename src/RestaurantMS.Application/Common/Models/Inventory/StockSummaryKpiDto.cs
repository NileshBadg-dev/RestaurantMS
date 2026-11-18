namespace RestaurantMS.Application.Common.Models.Inventory;

public record StockSummaryKpiDto(
    int TotalItems,
    decimal TotalStockValue,
    int LowStockCount,
    int OutOfStockCount,
    int ExpiringSoonCount);
