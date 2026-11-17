using Microsoft.Extensions.Caching.Memory;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;

namespace RestaurantMS.Infrastructure.Services;

public class SettingService : ISettingService
{
    private readonly ISettingRepository _settingRepository;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "HotelSettingsCacheKey";

    public SettingService(ISettingRepository settingRepository, IMemoryCache cache)
    {
        _settingRepository = settingRepository;
        _cache = cache;
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue = "", CancellationToken cancellationToken = default)
    {
        var settings = await GetAllAsync(cancellationToken);
        return settings.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val) ? val : defaultValue;
    }

    public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default!, CancellationToken cancellationToken = default)
    {
        var val = await GetSettingAsync(key, string.Empty, cancellationToken);
        if (string.IsNullOrEmpty(val))
        {
            return defaultValue;
        }

        try
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyDictionary<string, string>? cached) && cached is not null)
        {
            return cached;
        }

        var list = await _settingRepository.GetAllAsync(cancellationToken);
        var dict = list.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

        _cache.Set(CacheKey, (IReadOnlyDictionary<string, string>)dict, TimeSpan.FromMinutes(30));
        return dict;
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }
}
