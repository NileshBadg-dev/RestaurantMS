using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<AppUser?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsUserIdUniqueAsync(string userId, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().Where(u => u.UserId == userId);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<AppUser>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(u =>
                u.UserId.Contains(term) ||
                u.FullName.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AppUser>(items, totalCount, pageNumber, pageSize);
    }

    public void Add(AppUser user)
    {
        _context.Users.Add(user);
    }

    public void Update(AppUser user)
    {
        _context.Users.Update(user);
    }
}
