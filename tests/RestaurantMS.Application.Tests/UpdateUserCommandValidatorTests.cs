using FluentAssertions;
using RestaurantMS.Application.Users.Commands.UpdateUser;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateUserCommand(1L, "Updated Name", "user@example.com", "9876543210", 2L);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyFullName_FailsValidation(string fullName)
    {
        var command = new UpdateUserCommand(1L, fullName, null, null, 2L);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.FullName));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("plainaddress")]
    public void Validate_InvalidEmail_FailsValidation(string email)
    {
        var command = new UpdateUserCommand(1L, "Valid Name", email, null, 2L);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }

    [Theory]
    [InlineData("1234567890")] // starts with 1
    [InlineData("987654321")]  // 9 digits
    [InlineData("98765432100")] // 11 digits
    [InlineData("abcdefghij")]
    public void Validate_InvalidPhone_FailsValidation(string phone)
    {
        var command = new UpdateUserCommand(1L, "Valid Name", null, phone, 2L);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.PhoneNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_FailsValidation(long id)
    {
        var command = new UpdateUserCommand(id, "Valid Name", null, null, 2L);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidRoleId_FailsValidation(long roleId)
    {
        var command = new UpdateUserCommand(1L, "Valid Name", null, null, roleId);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.RoleId));
    }
}
