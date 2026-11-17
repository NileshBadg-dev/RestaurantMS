namespace RestaurantMS.Application.Common.Interfaces;

public interface IFeatureService
{
    Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, bool>> GetAllAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}
