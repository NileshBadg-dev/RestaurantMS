using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Common.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserIdUniqueAsync(string userId, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<AppUser>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    void Add(AppUser user);
    void Update(AppUser user);
}
