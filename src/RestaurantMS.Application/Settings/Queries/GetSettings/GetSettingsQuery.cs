using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Settings.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Settings.Queries.GetSettings;

public record GetSettingsQuery : IRequest<Result<HotelSettingsDto>>, IRequirePermission
{
    public string RequiredPermission => Permissions.Settings.View;
}
