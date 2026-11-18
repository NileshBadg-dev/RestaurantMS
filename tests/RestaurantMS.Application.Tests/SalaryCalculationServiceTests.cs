using FluentAssertions;
using RestaurantMS.Application.Common.Models.Staff;
using RestaurantMS.Application.Staff.Services;
using Xunit;

namespace RestaurantMS.Application.Tests;

public class SalaryCalculationServiceTests
{
    private readonly SalaryCalculationService _sut = new();

    [Fact]
    public void Calculate_MonthlySalaryWithAbsentHalfDayOvertimeAndAdvance_ReturnsExactMath()
    {
        // Basic = 30,000 / 30 days = 1,000/day; Hourly = 125/hr; OT 1.5x = 187.50/hr
        // 2 absent + 1 half-day = 2.5 days -> AbsentDeduction = 2,500
        // 4 hrs OT -> 4 * 187.50 = 750
        // Advance recovery = 2,000 (planned 2,000 of 5,000 outstanding)
        // Net = 30,000 + 750 - 2,500 - 2,000 = 26,250
        var input = new SalaryCalculationInput(
            SalaryType: "Monthly",
            BasicSalary: 30000m,
            TotalDaysInMonth: 30,
            PresentDays: 27,
            AbsentDays: 2,
            HalfDays: 1,
            LeaveDays: 0,
            WeekOffDays: 0,
            OvertimeHours: 4m,
            OvertimeMultiplier: 1.5m,
            OutstandingAdvance: 5000m,
            PlannedMonthlyInstalment: 2000m,
            AutoRecoverAdvance: true);

        var result = _sut.Calculate(input);

        result.BaseSalaryForMonth.Should().Be(30000m);
        result.OvertimePay.Should().Be(750m);
        result.AbsentDeduction.Should().Be(2500m);
        result.AdvanceRecovery.Should().Be(2000m);
        result.NetPayable.Should().Be(26250m);
    }

    [Fact]
    public void Calculate_DailySalaryType_ComputesBasicAndAbsentDeductionAccurately()
    {
        // Daily = 600/day * 30 days = 18,000 base; 3 absent + 2 half-days (4 days) = 2,400 deduction
        // Hourly = 75/hr; OT 1.5x = 112.50/hr; 2 hrs OT = 225
        // Auto-recover OFF -> 0
        // Net = 18,000 + 225 - 2,400 - 0 = 15,825
        var input = new SalaryCalculationInput(
            SalaryType: "Daily",
            BasicSalary: 600m,
            TotalDaysInMonth: 30,
            PresentDays: 25,
            AbsentDays: 3,
            HalfDays: 2,
            LeaveDays: 0,
            WeekOffDays: 0,
            OvertimeHours: 2m,
            OvertimeMultiplier: 1.5m,
            OutstandingAdvance: 3000m,
            PlannedMonthlyInstalment: 1000m,
            AutoRecoverAdvance: false);

        var result = _sut.Calculate(input);

        result.BaseSalaryForMonth.Should().Be(18000m);
        result.AbsentDeduction.Should().Be(2400m);
        result.OvertimePay.Should().Be(225m);
        result.AdvanceRecovery.Should().Be(0m);
        result.NetPayable.Should().Be(15825m);
    }

    [Fact]
    public void Calculate_WhenAdvanceExceedsEarnedSalary_CapsRecoveryAtEarnedSalary()
    {
        var input = new SalaryCalculationInput(
            SalaryType: "Monthly",
            BasicSalary: 10000m,
            TotalDaysInMonth: 30,
            PresentDays: 15,
            AbsentDays: 15,
            HalfDays: 0,
            LeaveDays: 0,
            WeekOffDays: 0,
            OvertimeHours: 0m,
            OvertimeMultiplier: 1.5m,
            OutstandingAdvance: 8000m,
            PlannedMonthlyInstalment: 8000m,
            AutoRecoverAdvance: true);

        var result = _sut.Calculate(input);

        result.AbsentDeduction.Should().Be(5000m);
        result.AdvanceRecovery.Should().Be(5000m);
        result.NetPayable.Should().Be(0m);
    }

    [Fact]
    public void BuildInstalmentSchedule_MonthlyPlanWithRemainder_GeneratesExactSchedule()
    {
        var schedule = _sut.BuildInstalmentSchedule(5000m, "MonthlyInstalment", 2000m, "2026-12");

        schedule.Count.Should().Be(3);
        schedule[0].MonthLabel.Should().Be("Dec 2026");
        schedule[0].InstalmentAmount.Should().Be(2000m);
        schedule[0].RemainingBalance.Should().Be(3000m);

        schedule[1].MonthLabel.Should().Be("Jan 2027");
        schedule[1].InstalmentAmount.Should().Be(2000m);
        schedule[1].RemainingBalance.Should().Be(1000m);

        schedule[2].MonthLabel.Should().Be("Feb 2027");
        schedule[2].InstalmentAmount.Should().Be(1000m);
        schedule[2].RemainingBalance.Should().Be(0m);
    }
}
