using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleReservationsUiData : IReservationsUiData
{
    private static readonly List<ReservationItemDto> ReservationsList =
    [
        new(1, "Sunil Gavaskar", "9822110099", "T3", "8:30 pm", 6, "Confirmed", "Corner table preferred"),
        new(2, "Sachin Tendulkar", "9822334455", "T8", "8:30 pm", 4, "Confirmed", "Birthday celebration cake"),
        new(3, "Rohit Sharma", "9822998877", "T10", "9:00 pm", 4, "Pending", "Family dinner"),
        new(4, "Ajinkya Rahane", "9822776655", "T6", "7:00 pm", 2, "Completed", "Quiet zone")
    ];

    public Task<PagedResult<ReservationItemDto>> GetReservationsAsync(DataTableRequest request, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var all = ReservationsList;
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim().ToLowerInvariant();
            all = all.Where(r => r.CustomerName.ToLowerInvariant().Contains(search) || r.TableCode.ToLowerInvariant().Contains(search)).ToList();
        }

        var paged = all.Skip(request.Start).Take(request.PageSize).ToList();
        return Task.FromResult(new PagedResult<ReservationItemDto>(paged, all.Count, request.PageNumber, request.PageSize));
    }

    public Task<IReadOnlyList<ReservationItemDto>> GetAllReservationsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ReservationItemDto>>(ReservationsList);
    }

    public Task CreateReservationAsync(string customerName, string mobile, string tableCode, string reservationTime, int guestsCount, string? specialRequest, CancellationToken cancellationToken = default)
    {
        ReservationsList.Add(new(ReservationsList.Count + 1, customerName, mobile, tableCode, reservationTime, guestsCount, "Confirmed", specialRequest));
        return Task.CompletedTask;
    }
}
