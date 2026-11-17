namespace RestaurantMS.Application.Common.Models.UiData;

public record KitchenTicketItemDto(
    string ItemName,
    int Quantity,
    string? Notes = null);
