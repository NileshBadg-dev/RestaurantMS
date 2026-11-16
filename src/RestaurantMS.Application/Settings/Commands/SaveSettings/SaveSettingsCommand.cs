using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Settings.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Settings.Commands.SaveSettings;

public record SaveSettingsCommand(
    string HotelNameEn,
    string HotelNameMr,
    string? LogoUrl = null,
    string? Address = null,
    string? ContactNumber = null,
    string? Email = null,
    string? Gstin = null,
    string? Fssai = null,
    string Currency = "INR",
    string Timezone = "Asia/Kolkata",
    string? ReceiptHeader = null,
    string? ReceiptFooter = null) : IRequest<Result<HotelSettingsDto>>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.Settings.Edit;
    public string EntityName => "HotelSettings";
    public string EntityId => "Global";
    public AuditAction Action => AuditAction.Update;
}
