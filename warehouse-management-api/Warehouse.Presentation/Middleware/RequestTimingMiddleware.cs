using System.Diagnostics;

namespace warehouse_management_api.Middleware;

public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    private const int SlowRequestThresholdMs = 1000;
    public async Task InvokeAsync(HttpContext context)
     {
         var stopwatch = Stopwatch.StartNew();
         context.Response.OnStarting(() =>
         {
             context.Response.Headers["X-Response-Time"] =
             $"{stopwatch.ElapsedMilliseconds}ms";
             return Task.CompletedTask;
         });
         await next(context);
         stopwatch.Stop();
         if (stopwatch.ElapsedMilliseconds >= SlowRequestThresholdMs)
         {
             logger.LogWarning("Slow request time: {RequestTime}ms", stopwatch.ElapsedMilliseconds);
         }
     }
}