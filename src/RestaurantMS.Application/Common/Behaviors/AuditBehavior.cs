using System.Text.Json;
using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.Application.Common.Repositories;
using RestaurantMS.Domain.Entities;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;

    public AuditBehavior(
        IAuditLogRepository auditLogRepository,
        ICurrentUser currentUser,
        IDateTime dateTime)
    {
        _auditLogRepository = auditLogRepository;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is IAuditableCommand auditableCommand)
        {
            var isSuccess = true;
            if (response is Result result)
            {
                isSuccess = result.IsSuccess;
            }

            if (isSuccess)
            {
                var auditLog = new AuditLog
                {
                    EntityName = auditableCommand.EntityName,
                    EntityId = auditableCommand.EntityId,
                    Action = auditableCommand.Action,
                    NewValues = JsonSerializer.Serialize(request),
                    CreatedBy = _currentUser.UserId ?? "System",
                    CreatedAt = _dateTime.UtcNow
                };

                await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            }
        }

        return response;
    }
}
