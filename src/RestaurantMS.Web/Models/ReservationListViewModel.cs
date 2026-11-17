using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class ReservationListViewModel
{
    public IReadOnlyList<ReservationItemDto> Reservations { get; set; } = Array.Empty<ReservationItemDto>();
    public int TodayReservationsCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int PendingCount { get; set; }
}
