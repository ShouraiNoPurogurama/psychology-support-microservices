using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    // [property: JsonProperty("temperature")]
    // double Temperature = 1.0,
    // [property: JsonProperty("topP")]
    // double TopP = 0.95,
    // [property: JsonProperty("maxOutputTokens")]
    // int MaxOutputTokens = 8192,
    [property: JsonProperty("response_schema")]
    object ResponseSchema,
    [property: JsonProperty("response_mime_type")]
    string ResponseMimeType = "application/json"
);
