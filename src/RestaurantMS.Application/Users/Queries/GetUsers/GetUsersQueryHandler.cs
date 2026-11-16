using MediatR;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Users.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var pagedUsers = await _userRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            cancellationToken);

        var userDtos = pagedUsers.Items.Select(u => new UserDto
        {
            Id = u.Id,
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            RoleName = u.Role?.Name ?? string.Empty,
            IsActive = u.IsActive,
            MustChangePassword = u.MustChangePassword,
            CreatedAt = u.CreatedAt
        }).ToList();

        var result = new PagedResult<UserDto>(userDtos, pagedUsers.TotalCount, pagedUsers.PageNumber, pagedUsers.PageSize);
        return Result.Success(result);
    }
}
