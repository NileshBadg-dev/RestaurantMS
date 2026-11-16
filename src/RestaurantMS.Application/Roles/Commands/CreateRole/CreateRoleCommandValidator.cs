using FluentValidation;
using RestaurantMS.Application.Common.Repositories;

namespace RestaurantMS.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(IRoleRepository roleRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.")
            .MustAsync(async (name, ct) => await roleRepository.IsNameUniqueAsync(name, null, ct))
            .WithMessage("Role name already exists.");
    }
}
