using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Application.Common.Interfaces.UiData;

public interface IKitchenUiData
{
    Task<KitchenDisplayDto> GetKitchenDisplayAsync(CancellationToken cancellationToken = default);
}
