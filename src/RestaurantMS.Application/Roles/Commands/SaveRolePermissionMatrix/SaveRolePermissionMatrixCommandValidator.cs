using FluentValidation;

namespace RestaurantMS.Application.Roles.Commands.SaveRolePermissionMatrix;

public class SaveRolePermissionMatrixCommandValidator : AbstractValidator<SaveRolePermissionMatrixCommand>
{
    public SaveRolePermissionMatrixCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Role Id is required.");
    }
}
