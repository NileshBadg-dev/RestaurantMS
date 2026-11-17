using FluentAssertions;
using RestaurantMS.Application.Settings.Commands.SaveSettings;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class SaveSettingsCommandValidatorTests
{
    private readonly SaveSettingsCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidSettings_ShouldPass()
    {
        // 27AAAAA0000A1Z2 is a valid GSTIN with correct Mod-36 checksum ('2')
        var command = new SaveSettingsCommand("Grand Restaurant", "भव्य रेस्टॉरंट", Gstin: "27AAAAA0000A1Z2");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyHotelNameEn_ShouldFail()
    {
        var command = new SaveSettingsCommand("", "भव्य रेस्टॉरंट");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "HotelNameEn");
    }

    [Fact]
    public void Validate_EmptyHotelNameMr_ShouldFail()
    {
        var command = new SaveSettingsCommand("Grand Restaurant", "");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "HotelNameMr");
    }

    [Theory]
    [InlineData("INVALIDGSTIN")]
    [InlineData("27AAAAA0000A1Z5")] // wrong checksum digit (expected 2)
    [InlineData("27AAAAA0000A1Z9")] // wrong checksum digit (expected 2)
    public void Validate_InvalidGstin_ShouldFail(string gstin)
    {
        var command = new SaveSettingsCommand("Grand Restaurant", "भव्य रेस्टॉरंट", Gstin: gstin);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Gstin");
    }
}
