using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiStructuredOutputGenerationConfigDto(
    [JsonProperty("response_schema")]
    object ResponseSchema,
    [JsonProperty("response_mime_type")]
    string ResponseMimeType = "application/json"
);
