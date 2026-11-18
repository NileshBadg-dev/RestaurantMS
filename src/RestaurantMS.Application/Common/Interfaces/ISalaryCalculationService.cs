using RestaurantMS.Application.Common.Models.Staff;

namespace RestaurantMS.Application.Common.Interfaces;

public interface ISalaryCalculationService
{
    SalaryCalculationResult Calculate(SalaryCalculationInput input);
    IReadOnlyList<AdvanceInstalmentPreviewDto> BuildInstalmentSchedule(
        decimal amount,
        string recoveryPlan,
        decimal? monthlyInstalmentAmount,
        string? startMonth);
}
