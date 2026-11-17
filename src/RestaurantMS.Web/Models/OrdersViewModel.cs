using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class OrdersViewModel
{
    public IReadOnlyList<OpenOrderDto> OpenOrders { get; set; } = new List<OpenOrderDto>();
    public int ActiveDineInOrders { get; set; }
    public int ActiveParcelOrders { get; set; }
    public decimal TotalRunningRevenue { get; set; }
}
