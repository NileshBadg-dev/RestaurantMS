using FluentAssertions;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Application.Staff.Validators;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class EmployeeAdvanceValidatorTests
{
    private readonly GiveEmployeeAdvanceRequestValidator _giveValidator = new();
    private readonly RecordAdvanceRecoveryRequestValidator _recoveryValidator = new();

    [Fact]
    public void GiveValidator_WhenAmountZeroOrNegative_FailsValidation()
    {
        var request = new GiveEmployeeAdvanceRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = 0m,
            PaymentMode = "Cash",
            Purpose = "Medical",
            RecoveryPlan = "OneTime"
        };

        var result = _giveValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GiveEmployeeAdvanceRequest.Amount));
    }

    [Fact]
    public void GiveValidator_WhenMonthlyInstalmentExceedsAmount_FailsValidation()
    {
        var request = new GiveEmployeeAdvanceRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = 3000m,
            PaymentMode = "UPI",
            Purpose = "School Fees",
            RecoveryPlan = "MonthlyInstalment",
            MonthlyInstalmentAmount = 4000m
        };

        var result = _giveValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GiveEmployeeAdvanceRequest.MonthlyInstalmentAmount));
    }

    [Fact]
    public void GiveValidator_WhenManagerOverrideWithoutReason_FailsValidation()
    {
        var request = new GiveEmployeeAdvanceRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = 25000m,
            PaymentMode = "Bank",
            Purpose = "Emergency",
            RecoveryPlan = "OneTime",
            IsManagerOverride = true,
            ManagerOverrideReason = ""
        };

        var result = _giveValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GiveEmployeeAdvanceRequest.ManagerOverrideReason));
    }

    [Fact]
    public void GiveValidator_WhenValidMonthlyPlanWithOverrideReason_PassesValidation()
    {
        var request = new GiveEmployeeAdvanceRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = 6000m,
            PaymentMode = "Bank",
            Purpose = "Hospital Bill",
            RecoveryPlan = "MonthlyInstalment",
            MonthlyInstalmentAmount = 2000m,
            StartMonth = "2026-12",
            IsManagerOverride = true,
            ManagerOverrideReason = "Approved by hotel owner for medical need"
        };

        var result = _giveValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecoveryValidator_WhenAmountNegativeOrInvalidMode_FailsValidation()
    {
        var request = new RecordAdvanceRecoveryRequest
        {
            StaffId = 1,
            Date = new DateTime(2026, 11, 18),
            Amount = -500m,
            PaymentMode = "Crypto"
        };

        var result = _recoveryValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecordAdvanceRecoveryRequest.Amount));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecordAdvanceRecoveryRequest.PaymentMode));
    }
}
