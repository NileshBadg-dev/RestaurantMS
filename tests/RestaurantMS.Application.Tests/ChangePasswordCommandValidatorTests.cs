using FluentAssertions;
using RestaurantMS.Application.Authentication.Commands.ChangePassword;

namespace RestaurantMS.Application.Tests;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidPasswordChange_ShouldPass()
    {
        var command = new ChangePasswordCommand("admin", "Admin@123", "NewSecure@123", "NewSecure@123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MismatchedPasswords_ShouldFail()
    {
        var command = new ChangePasswordCommand("admin", "Admin@123", "NewSecure@123", "Different@123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public void Validate_WeakNewPassword_ShouldFail()
    {
        var command = new ChangePasswordCommand("admin", "Admin@123", "weak", "weak");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }
}
