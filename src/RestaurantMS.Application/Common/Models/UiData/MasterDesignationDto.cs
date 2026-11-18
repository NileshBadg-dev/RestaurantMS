namespace RestaurantMS.Application.Common.Models.UiData;

public record MasterDesignationDto(
    long Id,
    string Title,
    string Department,
    string Level,
    int Headcount,
    string Status);
