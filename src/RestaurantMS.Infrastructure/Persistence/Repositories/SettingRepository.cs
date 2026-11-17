using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Repositories;

public class SettingRepository : ISettingRepository
{
    private readonly AppDbContext _context;

    public SettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<HotelSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.HotelSettings
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<HotelSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.HotelSettings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task SetSettingAsync(string key, string value, string group, CancellationToken cancellationToken = default)
    {
        var setting = await _context.HotelSettings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (setting is null)
        {
            setting = new HotelSetting
            {
                Key = key,
                Value = value,
                Group = group,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            _context.HotelSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.Group = group;
            setting.UpdatedAt = DateTime.UtcNow;
            _context.HotelSettings.Update(setting);
        }
    }

    public void Add(HotelSetting setting)
    {
        _context.HotelSettings.Add(setting);
    }

    public void Update(HotelSetting setting)
    {
        _context.HotelSettings.Update(setting);
    }
}
