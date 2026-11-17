using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Application.Common.Interfaces;

public interface IAuditableCommand
{
    string EntityName { get; }
    string EntityId { get; }
    AuditAction Action { get; }
}
