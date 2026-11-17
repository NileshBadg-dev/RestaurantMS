using FluentAssertions;
using RestaurantMS.Application.Authentication.Commands.ForgotPassword;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidUserId_Passes()
    {
        var command = new ForgotPasswordCommand("admin");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Validate_EmptyUserId_Fails(string userId)
    {
        var command = new ForgotPasswordCommand(userId);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ForgotPasswordCommand.UserId));
    }
}
