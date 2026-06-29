using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nexus404.Middleware;
using Nexus404.Middleware.Interfaces;
using Nexus404.Middleware.Models;
using Xunit;

namespace Nexus404.Tests;

public class Nexus404MiddlewareTests
{
    private static Nexus404Middleware CreateMiddleware(RequestDelegate next, IAiAnalysisService? analysisService = null)
    {
        analysisService ??= Mock.Of<IAiAnalysisService>();
        return new Nexus404Middleware(next, analysisService, NullLogger<Nexus404Middleware>.Instance);
    }

    [Fact]
    public async Task InvokeAsync_ContinuesPipeline_WhenStatusCodeIsNot404()
    {
        var analysisServiceMock = new Mock<IAiAnalysisService>();
        var middleware = CreateMiddleware(
            async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await Task.CompletedTask;
            },
            analysisServiceMock.Object
        );

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        analysisServiceMock.Verify(x => x.AnalyzeMissingPathAsync(It.IsAny<AnalysisRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_InterceptsAndAnalyzes_WhenStatusCodeIs404()
    {
        var analysisServiceMock = new Mock<IAiAnalysisService>();
        analysisServiceMock
            .Setup(x => x.AnalyzeMissingPathAsync(It.IsAny<AnalysisRequest>(), default))
            .ReturnsAsync(new FallbackResult());

        var middleware = CreateMiddleware(
            async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await Task.CompletedTask;
            },
            analysisServiceMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/not-found-page";

        await middleware.InvokeAsync(context);

        analysisServiceMock.Verify(x => x.AnalyzeMissingPathAsync(It.IsAny<AnalysisRequest>(), default), Times.Once);
    }
}
