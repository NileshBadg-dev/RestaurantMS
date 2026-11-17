using Microsoft.Extensions.Caching.Memory;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;

namespace RestaurantMS.Infrastructure.Services;

public class FeatureService : IFeatureService
{
    private readonly IFeatureToggleRepository _featureToggleRepository;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "FeatureTogglesCacheKey";

    public FeatureService(IFeatureToggleRepository featureToggleRepository, IMemoryCache cache)
    {
        _featureToggleRepository = featureToggleRepository;
        _cache = cache;
    }

    public async Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default)
    {
        var toggles = await GetAllAsync(cancellationToken);
        return toggles.TryGetValue(featureKey, out var isEnabled) && isEnabled;
    }

    public async Task<IReadOnlyDictionary<string, bool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyDictionary<string, bool>? cached) && cached is not null)
        {
            return cached;
        }

        var list = await _featureToggleRepository.GetAllAsync(cancellationToken);
        var dict = list.ToDictionary(t => t.Key, t => t.IsEnabled, StringComparer.OrdinalIgnoreCase);

        _cache.Set(CacheKey, (IReadOnlyDictionary<string, bool>)dict, TimeSpan.FromMinutes(30));
        return dict;
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }
}
