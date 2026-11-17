using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.Roles.Common;
using RestaurantMS.Domain.Entities;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
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

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsSystemRole = false,
            CreatedAt = _dateTime.UtcNow,
            CreatedBy = _currentUser.UserId ?? "System"
        };

        _roleRepository.Add(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = 0,
            PermissionCount = 0
        };

        return Result.Success(dto);
    }
}
