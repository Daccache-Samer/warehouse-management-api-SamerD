using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using warehouse_management_api.Error_response;
using warehouse_management_api.Resources;

namespace warehouse_management_api.Filters;

public class ModelValidationFilter(IStringLocalizer<SharedResources> localizer) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(e =>
                    localizer[e.ErrorMessage ?? string.Empty].Value).ToArray());

        var response = new ApiErrorResponse(
            ApiErrorCodes.ValidationFailed,
            localizer[SharedResources.ValidationErrorsOccurred],
            context.HttpContext.TraceIdentifier,
            errors);

        context.Result = new BadRequestObjectResult(response);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // We keep it empty as there is nothing to do after the action runs because validation only matters before it.
    }
}