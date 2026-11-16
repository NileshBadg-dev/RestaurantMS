using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Common.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(string name, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetWithPermissionsAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task UpdateRolePermissionsAsync(long roleId, IEnumerable<long> permissionIds, CancellationToken cancellationToken = default);
    void Add(Role role);
    void Update(Role role);
    void Delete(Role role);
}
