using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Common.Repositories;

public interface ISettingRepository
{
    Task<IReadOnlyList<HotelSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<HotelSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task SetSettingAsync(string key, string value, string group, CancellationToken cancellationToken = default);
    void Add(HotelSetting setting);
    void Update(HotelSetting setting);
}
