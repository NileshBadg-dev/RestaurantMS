namespace RestaurantMS.Application.Common.Models.UiData;

public record ActiveTableItemDto(
    string TableCode,
    string SectionName,
    string WaiterName,
    int GuestsCount,
    string SeatedAtTime,
    string TimeSpent,
    decimal RunningBill,
    string Status);
