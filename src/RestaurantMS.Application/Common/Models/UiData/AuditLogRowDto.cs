namespace RestaurantMS.Application.Common.Models.UiData;

public record AuditLogRowDto(
    long Id,
    string TimestampFormatted,
    string UserName,
    string RoleName,
    string Action,
    string Module,
    string Details,
    string IpAddress,
    string Severity
);
