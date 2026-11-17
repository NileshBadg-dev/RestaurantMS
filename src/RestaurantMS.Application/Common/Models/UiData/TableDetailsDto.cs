namespace RestaurantMS.Application.Common.Models.UiData;

public record TableDetailsDto(
    string TableCode,
    string Section,
    int Capacity,
    string Status,
    string? WaiterName,
    string? SeatedAtTime,
    decimal RunningBill,
    IReadOnlyList<BillingItemRowDto> Items);
