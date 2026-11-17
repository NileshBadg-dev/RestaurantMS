using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Common.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLog>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
