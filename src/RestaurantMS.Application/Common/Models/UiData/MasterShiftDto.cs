namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterShiftDto(
    long Id,
    string Name,
    string StartTime,
    string EndTime,
    int GracePeriodMinutes,
    int AssignedStaffCount,
    string Status);
