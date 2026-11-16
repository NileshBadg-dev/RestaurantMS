using FluentValidation;
using MediatR;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var firstFailure = failures[0];
            if (typeof(Result).IsAssignableFrom(typeof(TResponse)))
            {
                var error = new Error(firstFailure.ErrorCode ?? "ValidationError", firstFailure.ErrorMessage);
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

            throw new ValidationException(failures);
        }

        return await next();
    }
}
