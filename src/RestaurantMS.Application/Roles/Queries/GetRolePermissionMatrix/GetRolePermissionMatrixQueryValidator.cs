using FluentValidation;

namespace RestaurantMS.Application.Roles.Queries.GetRolePermissionMatrix;

public class GetRolePermissionMatrixQueryValidator : AbstractValidator<GetRolePermissionMatrixQuery>
{
    public GetRolePermissionMatrixQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Role Id is required.");
    }
}
