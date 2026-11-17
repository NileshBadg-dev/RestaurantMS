using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().OrderByDescending(a => a.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>(items, totalCount, pageNumber, pageSize);
    }
}
