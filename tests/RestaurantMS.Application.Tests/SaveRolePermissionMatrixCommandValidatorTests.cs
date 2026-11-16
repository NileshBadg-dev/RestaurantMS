using FluentAssertions;
using RestaurantMS.Application.Roles.Commands.SaveRolePermissionMatrix;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class SaveRolePermissionMatrixCommandValidatorTests
{
    private readonly SaveRolePermissionMatrixCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidRoleId_Passes()
    {
        var command = new SaveRolePermissionMatrixCommand(1L, [10L, 20L]);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidRoleId_Fails(long roleId)
    {
        var command = new SaveRolePermissionMatrixCommand(roleId, []);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaveRolePermissionMatrixCommand.RoleId));
    }
}
