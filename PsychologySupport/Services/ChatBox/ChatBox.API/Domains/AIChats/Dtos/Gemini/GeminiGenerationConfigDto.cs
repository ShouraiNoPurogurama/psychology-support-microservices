using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    [property: JsonProperty("temperature")]
    double Temperature = 1.0,
    [property: JsonProperty("topP")]
    double TopP = 0.95,
    [property: JsonProperty("maxOutputTokens")]
    int MaxOutputTokens = 8192
);
