using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiStructuredOutputGenerationConfigDto(
    [property: JsonProperty("response_schema")]
    object ResponseSchema,
    [property: JsonProperty("response_mime_type")]
    string ResponseMimeType = "application/json"
);
