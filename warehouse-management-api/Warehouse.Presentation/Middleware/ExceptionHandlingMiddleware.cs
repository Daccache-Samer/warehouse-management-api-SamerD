using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using warehouse_management_api.Error_response;
using warehouse_management_api.Resources;

namespace warehouse_management_api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IStringLocalizer<SharedResources> localizer)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, clientMessage, logLevel) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ApiErrorCodes.NotFound, exception.Message, LogLevel.Warning),
            ConflictException => (HttpStatusCode.Conflict, ApiErrorCodes.Conflict, exception.Message, LogLevel.Warning),
            ValidationException => (HttpStatusCode.BadRequest, ApiErrorCodes.ValidationFailed, exception.Message, LogLevel.Warning),
            DomainException => (HttpStatusCode.BadRequest, ApiErrorCodes.DomainRuleViolation, exception.Message, LogLevel.Warning),
            _ => (HttpStatusCode.InternalServerError, ApiErrorCodes.ServerError,
                localizer[SharedResources.UnexpectedError].Value, LogLevel.Error)
        };
        logger.Log(
            logLevel,
            exception,
            "Request {Method} {Path} failed with {ErrorCode} ({StatusCode}) [TraceId: {TraceId}]",
            context.Request.Method,
            context.Request.Path,
            errorCode,
            (int)statusCode,
            context.TraceIdentifier);

        var response = new ApiErrorResponse(errorCode, clientMessage, context.TraceIdentifier);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}