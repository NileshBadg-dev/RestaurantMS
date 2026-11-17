using System.Text.RegularExpressions;
using FluentValidation;

namespace RestaurantMS.Application.Settings.Commands.SaveSettings;

public class SaveSettingsCommandValidator : AbstractValidator<SaveSettingsCommand>
{
    private static readonly Regex GstinRegex = new(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", RegexOptions.Compiled);

    public SaveSettingsCommandValidator()
    {
        RuleFor(x => x.HotelNameEn)
            .NotEmpty().WithMessage("Hotel name (English) is required.")
            .MaximumLength(150).WithMessage("Hotel name (English) must not exceed 150 characters.");

        RuleFor(x => x.HotelNameMr)
            .NotEmpty().WithMessage("Hotel name (Marathi) is required.")
            .MaximumLength(150).WithMessage("Hotel name (Marathi) must not exceed 150 characters.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ContactNumber), () =>
        {
            RuleFor(x => x.ContactNumber)
                .Matches(@"^[6-9]\d{9}$").WithMessage("Contact number must be 10 digits starting with 6-9.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Gstin), () =>
        {
            RuleFor(x => x.Gstin)
                .Must(ValidGstin).WithMessage("Invalid GSTIN format or checksum.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Fssai), () =>
        {
            RuleFor(x => x.Fssai)
                .Matches(@"^\d{14}$").WithMessage("FSSAI number must be exactly 14 digits.");
        });
    }

    private static bool ValidGstin(string? gstin)
    {
        if (string.IsNullOrWhiteSpace(gstin))
        {
            return true;
        }

        var normalized = gstin.Trim().ToUpperInvariant();
        if (!GstinRegex.IsMatch(normalized))
        {
            return false;
        }

        return VerifyGstinChecksum(normalized);
    }

    private static bool VerifyGstinChecksum(string gstin)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var sum = 0;

        for (var i = 0; i < 14; i++)
        {
            var charValue = chars.IndexOf(gstin[i]);
            if (charValue < 0) return false;

            var weight = (i % 2 == 0) ? 1 : 2;
            var factor = charValue * weight;
            var quotient = factor / 36;
            var remainder = factor % 36;
            sum += quotient + remainder;
        }

        var checkValue = (36 - (sum % 36)) % 36;
        var expectedChar = chars[checkValue];

        return gstin[14] == expectedChar;
    }
}
