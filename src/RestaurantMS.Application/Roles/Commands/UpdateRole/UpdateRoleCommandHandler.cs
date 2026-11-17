using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        ICurrentUser currentUser,
        IDateTime dateTime,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            return Result.Failure<RoleDto>(new Error("Role.NotFound", "Role not found."));
        }

        if (role.IsSystemRole && !role.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<RoleDto>(new Error("Role.CannotRenameSystemRole", "System roles cannot be renamed."));
        }

        role.Name = request.Name.Trim();
        role.Description = request.Description?.Trim();
        role.UpdatedAt = _dateTime.UtcNow;
        role.UpdatedBy = _currentUser.UserId ?? "System";

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = role.Users.Count,
            PermissionCount = role.RolePermissions.Count
        };

        return Result.Success(dto);
    }
}
