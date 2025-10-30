using Newtonsoft.Json;

namespace Scheduling.API.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    [JsonProperty("response_schema")]
    object ResponseSchema,
    [JsonProperty("response_mime_type")]
    string ResponseMimeType = "application/json"
);
