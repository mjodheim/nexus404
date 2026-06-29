using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus404.Middleware.Interfaces;
using Nexus404.Middleware.Models;

namespace Nexus404.Middleware.Services;

public class PythonInteropService : IAiAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonInteropService> _logger;

    public PythonInteropService(HttpClient httpClient, IConfiguration configuration, ILogger<PythonInteropService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var pythonApiUrl = configuration["Nexus404:PythonServiceUrl"]
            ?? Environment.GetEnvironmentVariable("NEXUS404_PYTHON_URL")
            ?? "http://localhost:8000";
        _httpClient.BaseAddress = new Uri(pythonApiUrl);
    }

    public async Task<FallbackResult> AnalyzeMissingPathAsync(AnalysisRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/analyze", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<FallbackResult>(cancellationToken: cancellationToken);
            return result ?? new FallbackResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Python AI service for path: {Path}", request.AttemptedUrl);
            return new FallbackResult
            {
                ActionType = FallbackAction.None,
                ConfidenceScore = 0.0
            };
        }
    }
}
