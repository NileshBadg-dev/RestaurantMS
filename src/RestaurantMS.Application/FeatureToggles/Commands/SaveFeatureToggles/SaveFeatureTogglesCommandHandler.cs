using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.FeatureToggles.Commands.SaveFeatureToggles;

public class SaveFeatureTogglesCommandHandler : IRequestHandler<SaveFeatureTogglesCommand, Result>
{
    private readonly IFeatureToggleRepository _featureToggleRepository;
    private readonly IFeatureService _featureService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveFeatureTogglesCommandHandler(
        IFeatureToggleRepository featureToggleRepository,
        IFeatureService featureService,
        IUnitOfWork unitOfWork)
    {
        _featureToggleRepository = featureToggleRepository;
        _featureService = featureService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SaveFeatureTogglesCommand request, CancellationToken cancellationToken)
    {
        foreach (var (key, isEnabled) in request.Toggles)
        {
            await _featureToggleRepository.SetToggleAsync(key, isEnabled, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _featureService.InvalidateCache();

        return Result.Success();
    }
}
