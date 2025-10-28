using System.Text.Json.Serialization;

namespace Media.Application.Features.Media.Dtos.Gemini.ImageGeneration.Request;

public sealed class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContentReq> Contents { get; set; } = [];
}

public sealed class GeminiContentReq
{
    [JsonPropertyName("parts")]
    public List<GeminiPartReq> Parts { get; set; } = [];
}

public sealed class GeminiPartReq
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    // phải là inline_data (snake_case)
    [JsonPropertyName("inline_data")]
    public GeminiInlineDataReq? InlineData { get; set; }
}

public sealed class GeminiInlineDataReq
{
    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = "";

    [JsonPropertyName("data")]
    public string Data { get; set; } = "";
}