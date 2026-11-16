using MediatR;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Behaviors;

public class PermissionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;

    public PermissionBehavior(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IRequirePermission requirePermission)
        {
            var required = requirePermission.RequiredPermission;
            if (!_currentUser.Permissions.Contains(required))
            {
                var error = new Error("Auth.Forbidden", $"Permission '{required}' is required to perform this action.");
                if (typeof(Result).IsAssignableFrom(typeof(TResponse)))
                {
                    if (typeof(TResponse) == typeof(Result))
                    {
                        return (TResponse)(object)Result.Failure(error);
                    }

                    var genericType = typeof(TResponse).GetGenericArguments().FirstOrDefault();
                    if (genericType is not null)
                    {
                        var failureMethod = typeof(Result)
                            .GetMethods()
                            .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                            .MakeGenericMethod(genericType);
                        return (TResponse)failureMethod.Invoke(null, [error])!;
                    }
                }

                throw new UnauthorizedAccessException($"Permission '{required}' is required.");
            }
        }

        return await next();
    }
}
