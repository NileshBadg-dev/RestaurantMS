namespace RestaurantMS.Application.Common.Models.Staff;

public record AdvanceInstalmentPreviewDto(
    int InstalmentNumber,
    string MonthLabel,
    decimal InstalmentAmount,
    decimal RemainingBalance);
