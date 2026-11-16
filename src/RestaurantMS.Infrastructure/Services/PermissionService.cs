using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Infrastructure.Persistence;

namespace RestaurantMS.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var perms = await GetUserPermissionsAsync(userId, cancellationToken);
        return perms.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user?.Role is null)
        {
            return [];
        }

        return user.Role.RolePermissions
            .Where(rp => rp.Permission is not null)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList();
    }
}
