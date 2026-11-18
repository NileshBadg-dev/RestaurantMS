using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RestaurantMS.Application.Common.Interfaces;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Web.Authorization;

/// <summary>
/// Rejects access to controllers or actions when the required feature toggle is disabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class FeatureEnabledAttribute : Attribute, IAsyncActionFilter
{
    public string FeatureKey { get; }

    public FeatureEnabledAttribute(string featureKey)
    {
        FeatureKey = featureKey;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var featureService = context.HttpContext.RequestServices.GetService<IFeatureService>();
        if (featureService is not null)
        {
            var isEnabled = true;
            try
            {
                isEnabled = await featureService.IsEnabledAsync(FeatureKey, context.HttpContext.RequestAborted);
            }
            catch
            {
                isEnabled = true;
            }

            if (!isEnabled)
            {
                var isAjax = string.Equals(context.HttpContext.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                             || context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

                if (isAjax)
                {
                    context.Result = new JsonResult(Result.Failure("Feature.Disabled", $"Feature '{FeatureKey}' is currently disabled."))
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }

                context.Result = new ViewResult
                {
                    ViewName = "FeatureDisabled",
                    StatusCode = StatusCodes.Status403Forbidden,
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
                    {
                        ["DisabledFeatureKey"] = FeatureKey
                    }
                };
                return;
            }
        }

        await next();
    }
}
