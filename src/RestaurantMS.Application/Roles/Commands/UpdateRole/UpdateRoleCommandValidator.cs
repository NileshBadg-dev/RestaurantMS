using FluentValidation;
using RestaurantMS.Application.Common.Repositories;

namespace RestaurantMS.Application.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator(IRoleRepository roleRepository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Role Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.")
            .MustAsync(async (cmd, name, ct) => await roleRepository.IsNameUniqueAsync(name, cmd.Id, ct))
            .WithMessage("Role name already exists.");
    }
}
