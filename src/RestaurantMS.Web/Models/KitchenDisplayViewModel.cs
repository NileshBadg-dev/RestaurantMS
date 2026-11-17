using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class KitchenDisplayViewModel
{
    public KitchenDisplayDto DisplayData { get; set; } = new(
        Array.Empty<KitchenTicketDto>(),
        Array.Empty<KitchenTicketDto>(),
        Array.Empty<KitchenTicketDto>());
    public int AutoRefreshSeconds { get; set; } = 15;
    public string SelectedStation { get; set; } = "All";
}
