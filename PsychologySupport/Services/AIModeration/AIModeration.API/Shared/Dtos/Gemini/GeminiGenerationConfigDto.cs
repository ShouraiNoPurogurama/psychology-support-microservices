using Newtonsoft.Json;

namespace AIModeration.API.Shared.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    [JsonProperty("response_schema")]
    object ResponseSchema,
    [JsonProperty("response_mime_type")]
    string ResponseMimeType = "application/json"
);
