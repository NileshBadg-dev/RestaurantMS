using MediatR;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Queries.GetRolePermissionMatrix;

public class GetRolePermissionMatrixQueryHandler : IRequestHandler<GetRolePermissionMatrixQuery, Result<RolePermissionMatrixDto>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolePermissionMatrixQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RolePermissionMatrixDto>> Handle(GetRolePermissionMatrixQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<RolePermissionMatrixDto>(new Error("Role.NotFound", "Role not found."));
        }

        var allPermissions = await _roleRepository.GetAllPermissionsAsync(cancellationToken);
        var grantedPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        var screens = allPermissions
            .Select(p => p.Screen)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        var rows = new List<ScreenPermissionRowDto>();
        foreach (var screen in screens)
        {
            var screenPerms = allPermissions.Where(p => p.Screen == screen).ToList();

            rows.Add(new ScreenPermissionRowDto
            {
                Screen = screen,
                View = CreateCell(screenPerms, Permissions.Actions.View, grantedPermissionIds),
                Create = CreateCell(screenPerms, Permissions.Actions.Create, grantedPermissionIds),
                Edit = CreateCell(screenPerms, Permissions.Actions.Edit, grantedPermissionIds),
                Delete = CreateCell(screenPerms, Permissions.Actions.Delete, grantedPermissionIds),
                Print = CreateCell(screenPerms, Permissions.Actions.Print, grantedPermissionIds),
                Export = CreateCell(screenPerms, Permissions.Actions.Export, grantedPermissionIds)
            });
        }

        var matrix = new RolePermissionMatrixDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Rows = rows
        };

        return Result.Success(matrix);
    }

    private static PermissionCellDto? CreateCell(
        IEnumerable<Domain.Entities.Permission> permissions,
        string action,
        HashSet<long> grantedIds)
    {
        var perm = permissions.FirstOrDefault(p => p.Action.Equals(action, StringComparison.OrdinalIgnoreCase));
        if (perm is null)
        {
            return null;
        }

        return new PermissionCellDto
        {
            PermissionId = perm.Id,
            Code = perm.Code,
            IsGranted = grantedIds.Contains(perm.Id)
        };
    }
}
