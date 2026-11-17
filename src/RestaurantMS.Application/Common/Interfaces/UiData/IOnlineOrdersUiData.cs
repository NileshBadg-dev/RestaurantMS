using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IOnlineOrdersUiData
{
    Task<IReadOnlyList<OnlineOrderDto>> GetOnlineOrdersAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, CancellationToken cancellationToken = default);
}
