using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace warehouse_management_api.Filters;

public class ActionLoggingFilter(ILogger<ActionLoggingFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        logger.LogInformation(
            "Executing {Action} with parameters [{Parameters}] [TraceId: {TraceId}]",
            context.ActionDescriptor.DisplayName,
            string.Join(", ", context.ActionArguments.Keys),
            context.HttpContext.TraceIdentifier);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            // ExceptionHandlingMiddleware already logs failures once the exception
            // propagates there — logging it again here would duplicate the entry.
            return;
        }

        var statusCode = (context.Result as IStatusCodeActionResult)?.StatusCode
                         ?? context.HttpContext.Response.StatusCode;

        logger.LogInformation(
            "Executed {Action} -> {StatusCode} [TraceId: {TraceId}]",
            context.ActionDescriptor.DisplayName,
            statusCode,
            context.HttpContext.TraceIdentifier);
    }
}