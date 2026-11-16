using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Common.Repositories;

public interface IFeatureToggleRepository
{
    Task<IReadOnlyList<FeatureToggle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FeatureToggle?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task SetToggleAsync(string key, bool isEnabled, CancellationToken cancellationToken = default);
}
