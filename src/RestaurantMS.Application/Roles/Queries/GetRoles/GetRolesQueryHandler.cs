using MediatR;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);

        var dtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSystemRole = r.IsSystemRole,
            UserCount = r.Users.Count,
            PermissionCount = r.RolePermissions.Count
        }).ToList();

        return Result.Success<IReadOnlyList<RoleDto>>(dtos);
    }
}
