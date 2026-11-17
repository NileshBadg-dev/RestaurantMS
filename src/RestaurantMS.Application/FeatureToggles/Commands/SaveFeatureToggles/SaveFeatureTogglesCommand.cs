using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Domain.Enums;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.FeatureToggles.Commands.SaveFeatureToggles;

public record SaveFeatureTogglesCommand(Dictionary<string, bool> Toggles)
    : IRequest<Result>, IRequirePermission, IAuditableCommand
{
    public string RequiredPermission => Permissions.FeatureToggles.Edit;
    public string EntityName => "FeatureToggles";
    public string EntityId => "All";
    public AuditAction Action => AuditAction.Update;
}
