namespace RestaurantMS.Application.Common.Models.UiData;

public record KitchenDisplayDto(
    IReadOnlyList<KitchenTicketDto> NewTickets,
    IReadOnlyList<KitchenTicketDto> PreparingTickets,
    IReadOnlyList<KitchenTicketDto> ReadyTickets);
