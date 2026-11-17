using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.FeatureToggles.Common;
using RestaurantMS.Domain.Constants;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.FeatureToggles.Queries.GetFeatureToggles;

public record GetFeatureTogglesQuery : IRequest<Result<IReadOnlyList<FeatureToggleDto>>>, IRequirePermission
{
    public string RequiredPermission => Permissions.FeatureToggles.View;
}
