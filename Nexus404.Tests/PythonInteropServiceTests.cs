using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus404.Middleware.Interfaces;
using Nexus404.Middleware.Models;
using Nexus404.Middleware.Services;
using Xunit;

namespace Nexus404.Tests;

public class PythonInteropServiceTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _responseMessage;
        private readonly Exception? _exceptionToThrow;

        public MockHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }

        public MockHttpMessageHandler(Exception exceptionToThrow)
        {
            _exceptionToThrow = exceptionToThrow;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_exceptionToThrow != null)
                throw _exceptionToThrow;

            return Task.FromResult(_responseMessage!);
        }
    }

    private static PythonInteropService CreateService(HttpClient httpClient)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Nexus404:PythonServiceUrl"] = "http://localhost:8000"
            })
            .Build();

        return new PythonInteropService(httpClient, config, NullLogger<PythonInteropService>.Instance);
    }

    [Fact]
    public async Task AnalyzeMissingPathAsync_ReturnsFallbackResult_WhenApiCallSucceeds()
    {
        var expected = new FallbackResult
        {
            ActionType = FallbackAction.Redirect,
            SuggestedUrl = "/home",
            ConfidenceScore = 0.95
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expected))
        };

        var httpClient = new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("http://localhost:8000/")
        };

        var service = CreateService(httpClient);
        var request = new AnalysisRequest { AttemptedUrl = "/old-page", Method = "GET" };

        var result = await service.AnalyzeMissingPathAsync(request);

        Assert.NotNull(result);
        Assert.Equal(FallbackAction.Redirect, result.ActionType);
        Assert.Equal("/home", result.SuggestedUrl);
    }

    [Fact]
    public async Task AnalyzeMissingPathAsync_ReturnsDefault_WhenApiCallFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("http://localhost:8000/")
        };

        var service = CreateService(httpClient);
        var request = new AnalysisRequest { AttemptedUrl = "/fail" };

        var result = await service.AnalyzeMissingPathAsync(request);

        Assert.NotNull(result);
        Assert.Equal(FallbackAction.None, result.ActionType);
        Assert.Equal(0.0, result.ConfidenceScore);
    }

    [Fact]
    public async Task AnalyzeMissingPathAsync_ReturnsDefault_OnHttpRequestException()
    {
        var httpClient = new HttpClient(new MockHttpMessageHandler(new HttpRequestException("Network failure")))
        {
            BaseAddress = new Uri("http://localhost:8000/")
        };

        var service = CreateService(httpClient);
        var request = new AnalysisRequest { AttemptedUrl = "/error" };

        var result = await service.AnalyzeMissingPathAsync(request);

        Assert.NotNull(result);
        Assert.Equal(FallbackAction.None, result.ActionType);
    }
}
