using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IReservationsUiData
{
    Task<PagedResult<ReservationItemDto>> GetReservationsAsync(DataTableRequest request, DateTime? date = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReservationItemDto>> GetAllReservationsAsync(CancellationToken cancellationToken = default);
    Task CreateReservationAsync(string customerName, string mobile, string tableCode, string reservationTime, int guestsCount, string? specialRequest, CancellationToken cancellationToken = default);
}
