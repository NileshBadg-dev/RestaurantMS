using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class OnlineOrdersViewModel
{
    public IReadOnlyList<OnlineOrderDto> Orders { get; init; } = Array.Empty<OnlineOrderDto>();
    public int NewOrdersCount => Orders.Count(o => o.Status == "New");
    public int PreparingCount => Orders.Count(o => o.Status == "InPreparation");
    public int ReadyCount => Orders.Count(o => o.Status == "FoodReady");
    public int OutForDeliveryCount => Orders.Count(o => o.Status == "OutForDelivery");
    public int DeliveredCount => Orders.Count(o => o.Status == "Delivered");
    public decimal TotalRevenue => Orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
    public decimal NetPayout => Orders.Where(o => o.Status != "Cancelled").Sum(o => o.NetPayout);
}
