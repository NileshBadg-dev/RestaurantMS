using System.Globalization;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Application.Staff.Services;

public class SalaryCalculationService : ISalaryCalculationService
{
    public SalaryCalculationResult Calculate(SalaryCalculationInput input)
    {
        var daysInMonth = Math.Max(1, input.TotalDaysInMonth);
        var isDaily = string.Equals(input.SalaryType, "Daily", StringComparison.OrdinalIgnoreCase);

        var baseSalaryForMonth = isDaily
            ? Math.Round(input.BasicSalary * daysInMonth, 2, MidpointRounding.AwayFromZero)
            : Math.Round(input.BasicSalary, 2, MidpointRounding.AwayFromZero);

        var perDayRate = isDaily ? input.BasicSalary : input.BasicSalary / daysInMonth;
        var absentEquivalentDays = Math.Max(0m, input.AbsentDays) + (Math.Max(0m, input.HalfDays) * 0.5m);
        var absentDeduction = Math.Round(perDayRate * absentEquivalentDays, 2, MidpointRounding.AwayFromZero);

        var multiplier = input.OvertimeMultiplier > 0 ? input.OvertimeMultiplier : 1.5m;
        var hourlyRate = perDayRate / 8m;
        var overtimePay = Math.Round(Math.Max(0m, input.OvertimeHours) * hourlyRate * multiplier, 2, MidpointRounding.AwayFromZero);

        var grossAfterDeductions = Math.Max(0m, baseSalaryForMonth + overtimePay - absentDeduction);

        var advanceRecovery = 0m;
        if (input.AutoRecoverAdvance && input.OutstandingAdvance > 0m)
        {
            var planned = input.PlannedMonthlyInstalment > 0m
                ? Math.Min(input.PlannedMonthlyInstalment, input.OutstandingAdvance)
                : input.OutstandingAdvance;

            advanceRecovery = Math.Round(Math.Min(planned, grossAfterDeductions), 2, MidpointRounding.AwayFromZero);
        }

        var netPayable = Math.Round(baseSalaryForMonth + overtimePay - absentDeduction - advanceRecovery, 2, MidpointRounding.AwayFromZero);
        if (netPayable < 0m)
        {
            netPayable = 0m;
        }

        return new SalaryCalculationResult(
            baseSalaryForMonth,
            overtimePay,
            absentDeduction,
            advanceRecovery,
            netPayable);
    }

    public IReadOnlyList<AdvanceInstalmentPreviewDto> BuildInstalmentSchedule(
        decimal amount,
        string recoveryPlan,
        decimal? monthlyInstalmentAmount,
        string? startMonth)
    {
        var schedule = new List<AdvanceInstalmentPreviewDto>();
        if (amount <= 0m)
        {
            return schedule;
        }

        var startDate = ParseStartMonth(startMonth);
        var isMonthly = string.Equals(recoveryPlan, "MonthlyInstalment", StringComparison.OrdinalIgnoreCase);

        if (!isMonthly || !monthlyInstalmentAmount.HasValue || monthlyInstalmentAmount.Value <= 0m || monthlyInstalmentAmount.Value >= amount)
        {
            schedule.Add(new AdvanceInstalmentPreviewDto(
                1,
                startDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Math.Round(amount, 2, MidpointRounding.AwayFromZero),
                0m));
            return schedule;
        }

        var remaining = amount;
        var instalment = Math.Round(monthlyInstalmentAmount.Value, 2, MidpointRounding.AwayFromZero);
        var index = 1;

        while (remaining > 0m && index <= 60)
        {
            var currentPay = Math.Min(instalment, remaining);
            remaining = Math.Round(remaining - currentPay, 2, MidpointRounding.AwayFromZero);
            var monthDate = startDate.AddMonths(index - 1);

            schedule.Add(new AdvanceInstalmentPreviewDto(
                index,
                monthDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                currentPay,
                remaining));

            index++;
        }

        return schedule;
    }

    private static DateTime ParseStartMonth(string? startMonth)
    {
        if (!string.IsNullOrWhiteSpace(startMonth) &&
            DateTime.TryParseExact(startMonth + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        var now = DateTime.UtcNow;
        return new DateTime(now.Year, now.Month, 1).AddMonths(1);
    }
}
