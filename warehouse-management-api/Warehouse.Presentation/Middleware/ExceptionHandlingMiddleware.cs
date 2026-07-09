using System.Net;
using System.Text.Json;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Exceptions;

namespace warehouse_management_api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message);
        }
    }

    private static Task WriteProblem(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var payload = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(payload);
    }
}