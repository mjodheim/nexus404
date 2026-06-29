using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexus404.Middleware.Interfaces;
using Nexus404.Middleware.Models;

namespace Nexus404.Middleware;

public class Nexus404Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<Nexus404Middleware> _logger;
    private readonly IAiAnalysisService _analysisService;

    public Nexus404Middleware(RequestDelegate next, IAiAnalysisService analysisService, ILogger<Nexus404Middleware> logger)
    {
        _next = next;
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status404NotFound && !context.Response.HasStarted)
        {
            _logger.LogWarning("404 Not Found intercepted for path: {Path}", context.Request.Path);

            var request = new AnalysisRequest
            {
                AttemptedUrl = context.Request.Path,
                Referer = context.Request.Headers["Referer"],
                Method = context.Request.Method,
                UserAgent = context.Request.Headers["User-Agent"],
                Timestamp = DateTime.UtcNow
            };

            var result = await _analysisService.AnalyzeMissingPathAsync(request);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";

            var response = JsonSerializer.Serialize(new
            {
                status = 404,
                error = "Not Found",
                message = "The requested resource was not found.",
                suggestion = result.SuggestedUrl,
                action = result.ActionType.ToString()
            });

            await context.Response.WriteAsync(response);
        }
    }
}
