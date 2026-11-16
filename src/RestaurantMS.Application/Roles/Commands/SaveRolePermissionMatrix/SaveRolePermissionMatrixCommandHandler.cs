using MediatR;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Commands.SaveRolePermissionMatrix;

public class SaveRolePermissionMatrixCommandHandler : IRequestHandler<SaveRolePermissionMatrixCommand, Result>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveRolePermissionMatrixCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SaveRolePermissionMatrixCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", "Role not found."));
        }

        await _roleRepository.UpdateRolePermissionsAsync(request.RoleId, request.PermissionIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
