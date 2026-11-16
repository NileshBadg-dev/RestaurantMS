using FluentAssertions;
using RestaurantMS.Application.FeatureToggles.Commands.SaveFeatureToggles;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class SaveFeatureTogglesCommandValidatorTests
{
    private readonly SaveFeatureTogglesCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidToggles_Passes()
    {
        var command = new SaveFeatureTogglesCommand(new Dictionary<string, bool> { ["Billing.Discounts"] = true });
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullToggles_Fails()
    {
        var command = new SaveFeatureTogglesCommand(null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaveFeatureTogglesCommand.Toggles));
    }
}
