using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using warehouse_management_api.Middleware;

namespace Warehouse.Api.UnitTests.Middleware;

public class RequestTimingMiddlewareTests
{
    private readonly Mock<ILogger<RequestTimingMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_AddsXResponseTimeHeaderToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var middleware = new RequestTimingMiddleware(Next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.ContainsKey("X-Response-Time").Should().BeTrue();
        context.Response.Headers["X-Response-Time"].ToString().Should().EndWith("ms");
        return;

        Task Next(HttpContext ctx) => Task.CompletedTask;
    }

    [Fact]
    public async Task InvokeAsync_WhenRequestIsSlow_LogsWarning()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Request =
            {
                Method = "GET",
                Path = "/api/products"
            }
        };

        var middleware = new RequestTimingMiddleware(Next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Verify that a warning log was triggered due to slow request threshold
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow Request Detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return;

        // Simulate a slow request by making the next delegate delay execution (> 500ms)
        async Task Next(HttpContext ctx)
        {
            await Task.Delay(550);
        }
    }

    [Fact]
    public async Task InvokeAsync_WhenRequestIsFast_DoesNotLogWarning()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var middleware = new RequestTimingMiddleware(Next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert 
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Slow Request Detected")), // Changed IsAny to Is here
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
        return;

        Task Next(HttpContext ctx) => Task.CompletedTask;
    }
}