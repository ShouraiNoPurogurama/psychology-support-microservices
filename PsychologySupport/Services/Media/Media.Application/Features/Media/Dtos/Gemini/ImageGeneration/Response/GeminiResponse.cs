using System.Text.Json.Serialization;

namespace Media.Application.Features.Media.Dtos.Gemini.ImageGeneration.Response;

public sealed class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

public sealed class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContentResp? Content { get; set; }
}

public sealed class GeminiContentResp
{
    [JsonPropertyName("parts")]
    public List<GeminiPartResp>? Parts { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public sealed class GeminiPartResp
{
    // response dùng inlineData (camelCase)
    [JsonPropertyName("inlineData")]
    public GeminiInlineDataResp? InlineData { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public sealed class GeminiInlineDataResp
{
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; } // ví dụ "image/png"

    [JsonPropertyName("data")]
    public string? Data { get; set; }     // base64 ảnh
}