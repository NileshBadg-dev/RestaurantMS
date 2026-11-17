namespace RestaurantMS.Application.Common.Models.UiData;

public record KitchenTicketDto(
    string TicketId,
    string KotNumber,
    string TableOrTakeaway,
    string ElapsedFormatted,
    bool IsLate,
    bool IsParcel,
    string Status,
    IReadOnlyList<KitchenTicketItemDto> Items);
