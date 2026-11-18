using System.Text.RegularExpressions;

namespace RestaurantMS.Application.Common.Validation;

/// <summary>
/// Shared Indian statutory and POS format validation helpers (Mobile, Email, GSTIN checksum, PAN, UPI VPA, decimals).
/// </summary>
public static class IndianValidationRules
{
    private static readonly Regex MobileRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);
    private static readonly Regex PanRegex = new(@"^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.Compiled);
    private static readonly Regex GstinRegex = new(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", RegexOptions.Compiled);
    private static readonly Regex UpiIdRegex = new(@"^[a-zA-Z0-9.\-_]{2,256}@[a-zA-Z]{2,64}$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex ShortCodeRegex = new(@"^[A-Za-z0-9\-_]{2,16}$", RegexOptions.Compiled);

    public static bool IsValidMobile(string? mobile) =>
        !string.IsNullOrWhiteSpace(mobile) && MobileRegex.IsMatch(mobile.Trim());

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

    public static bool IsValidPan(string? pan) =>
        !string.IsNullOrWhiteSpace(pan) && PanRegex.IsMatch(pan.Trim().ToUpperInvariant());

    public static bool IsValidUpiId(string? upiId) =>
        !string.IsNullOrWhiteSpace(upiId) && UpiIdRegex.IsMatch(upiId.Trim());

    public static bool IsValidShortCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && ShortCodeRegex.IsMatch(code.Trim());

    public static bool HasMaxDecimals(decimal value, int maxPlaces)
    {
        var factor = (decimal)Math.Pow(10, maxPlaces);
        return Math.Truncate(value * factor) == value * factor;
    }

    public static bool IsValidGstin(string? gstin)
    {
        if (string.IsNullOrWhiteSpace(gstin))
        {
            return false;
        }

        var normalized = gstin.Trim().ToUpperInvariant();
        if (!GstinRegex.IsMatch(normalized))
        {
            return false;
        }

        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var sum = 0;
        for (var i = 0; i < 14; i++)
        {
            var charValue = chars.IndexOf(normalized[i]);
            if (charValue < 0) return false;
            var factor = charValue * (i % 2 == 0 ? 1 : 2);
            sum += (factor / 36) + (factor % 36);
        }

        var expectedChar = chars[(36 - (sum % 36)) % 36];
        return normalized[14] == expectedChar;
    }
}
