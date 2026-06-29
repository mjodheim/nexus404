using System;
using System.Text.Json.Serialization;

namespace Nexus404.Middleware.Models;

public class AnalysisRequest
{
    [JsonPropertyName("attemptedUrl")]
    public string AttemptedUrl { get; set; } = string.Empty;

    [JsonPropertyName("referer")]
    public string? Referer { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
