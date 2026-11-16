using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Repositories;

public class FeatureToggleRepository : IFeatureToggleRepository
{
    private readonly AppDbContext _context;

    public FeatureToggleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FeatureToggle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureToggles
            .AsNoTracking()
            .OrderBy(t => t.Group)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FeatureToggle?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureToggles
            .FirstOrDefaultAsync(t => t.Key == key, cancellationToken);
    }

    public async Task SetToggleAsync(string key, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var toggle = await _context.FeatureToggles
            .FirstOrDefaultAsync(t => t.Key == key, cancellationToken);

        if (toggle is not null)
        {
            toggle.IsEnabled = isEnabled;
            toggle.UpdatedAt = DateTime.UtcNow;
            _context.FeatureToggles.Update(toggle);
        }
    }
}
