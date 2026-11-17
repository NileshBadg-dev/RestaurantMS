using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class ParcelListViewModel
{
    public IReadOnlyList<ParcelItemDto> ActiveParcels { get; set; } = new List<ParcelItemDto>();
    public int TotalPreparing { get; set; }
    public int TotalReady { get; set; }
    public int TotalDeliveredToday { get; set; }
    public decimal TotalParcelSalesToday { get; set; }
}
