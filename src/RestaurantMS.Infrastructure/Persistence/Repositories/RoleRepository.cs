using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.AsNoTracking().Where(r => r.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .Include(r => r.RolePermissions)
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetWithPermissionsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Screen)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRolePermissionsAsync(long roleId, IEnumerable<long> permissionIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(existing);

        var newRolePermissions = permissionIds.Distinct().Select(permId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permId
        });

        await _context.RolePermissions.AddRangeAsync(newRolePermissions, cancellationToken);
    }

    public void Add(Role role)
    {
        _context.Roles.Add(role);
    }

    public void Update(Role role)
    {
        _context.Roles.Update(role);
    }

    public void Delete(Role role)
    {
        role.IsDeleted = true;
        _context.Roles.Update(role);
    }
}
