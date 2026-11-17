namespace RestaurantMS.Application.Common.Models.UiData;

public record OpenOrderDto(
    string OrderNumber,
    string TableOrType,
    string WaiterName,
    string OrderedAt,
    int ItemsCount,
    decimal RunningAmount,
    string Status);
