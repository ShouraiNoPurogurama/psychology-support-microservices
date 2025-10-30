using Newtonsoft.Json;

namespace Translation.API.Domains.Translations.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    // [JsonProperty("temperature")]
    // double Temperature = 1.0,
    // [JsonProperty("topP")]
    // double TopP = 0.95,
    // [JsonProperty("maxOutputTokens")]
    // int MaxOutputTokens = 8192,
    [JsonProperty("response_schema")]
    object ResponseSchema,
    [JsonProperty("response_mime_type")]
    string ResponseMimeType = "application/json"
);
