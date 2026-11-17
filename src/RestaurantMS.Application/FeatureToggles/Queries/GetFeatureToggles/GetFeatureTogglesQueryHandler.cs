using MediatR;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Application.FeatureToggles.Common;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.FeatureToggles.Queries.GetFeatureToggles;

public class GetFeatureTogglesQueryHandler : IRequestHandler<GetFeatureTogglesQuery, Result<IReadOnlyList<FeatureToggleDto>>>
{
    private readonly IFeatureToggleRepository _featureToggleRepository;

    public GetFeatureTogglesQueryHandler(IFeatureToggleRepository featureToggleRepository)
    {
        _featureToggleRepository = featureToggleRepository;
    }

    public async Task<Result<IReadOnlyList<FeatureToggleDto>>> Handle(GetFeatureTogglesQuery request, CancellationToken cancellationToken)
    {
        var toggles = await _featureToggleRepository.GetAllAsync(cancellationToken);

        var dtos = toggles.Select(t => new FeatureToggleDto
        {
            Key = t.Key,
            Name = t.Name,
            Description = t.Description,
            Group = t.Group,
            IsEnabled = t.IsEnabled
        }).ToList();

        return Result.Success<IReadOnlyList<FeatureToggleDto>>(dtos);
    }
}
