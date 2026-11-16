namespace RestaurantMS.Application.Common.Interfaces;

public interface ISettingService
{
    Task<string> GetSettingAsync(string key, string defaultValue = "", CancellationToken cancellationToken = default);
    Task<T> GetSettingAsync<T>(string key, T defaultValue = default!, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}
