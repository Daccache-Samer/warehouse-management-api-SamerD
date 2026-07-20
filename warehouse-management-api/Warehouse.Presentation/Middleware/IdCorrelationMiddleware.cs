namespace warehouse_management_api.Middleware;

public class IdCorrelationMiddleware(RequestDelegate next, ILogger<IdCorrelationMiddleware> logger)
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName];
        // ReSharper disable once NullableWarningSuppressionIsUsed
        context.TraceIdentifier = correlationId! ;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });
        var state = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        };
        using (logger.BeginScope(state))
        {
            await next(context);
        }
    }
}