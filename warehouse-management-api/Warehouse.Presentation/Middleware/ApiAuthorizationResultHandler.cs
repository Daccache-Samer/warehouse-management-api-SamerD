using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using warehouse_management_api.Error_response;

namespace warehouse_management_api.Middleware;

public class ApiAuthorizationResultHandler(ILogger<ApiAuthorizationResultHandler> logger)
    : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        var (statusCode, errorCode, message, logLevel) = authorizeResult.Challenged
            ? (HttpStatusCode.Unauthorized, ApiErrorCodes.Unauthorized,
                "Authentication is required to access this resource.", LogLevel.Warning)
            : (HttpStatusCode.Forbidden, ApiErrorCodes.Forbidden,
                "You do not have permission to perform this action.", LogLevel.Warning);

        logger.Log(
            logLevel,
            "Request {Method} {Path} failed with {ErrorCode} ({StatusCode}) [TraceId: {TraceId}]",
            context.Request.Method,
            context.Request.Path,
            errorCode,
            (int)statusCode,
            context.TraceIdentifier);

        var response = new ApiErrorResponse(errorCode, message, context.TraceIdentifier);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}