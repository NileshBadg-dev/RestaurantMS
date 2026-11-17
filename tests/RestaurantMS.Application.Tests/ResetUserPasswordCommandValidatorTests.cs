using FluentAssertions;
using RestaurantMS.Application.Users.Commands.ResetUserPassword;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class ResetUserPasswordCommandValidatorTests
{
    private readonly ResetUserPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidPassword_Passes()
    {
        var command = new ResetUserPasswordCommand(1L, "Secure@123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("short1A")] // < 8
    [InlineData("alllowercase1")] // no upper
    [InlineData("ALLUPPERCASE1")] // no lower
    [InlineData("NoDigitsHere!")] // no digit
    public void Validate_WeakPassword_Fails(string password)
    {
        var command = new ResetUserPasswordCommand(1L, password);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetUserPasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_Fails(long id)
    {
        var command = new ResetUserPasswordCommand(id, "Valid123Password");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetUserPasswordCommand.Id));
    }
}
