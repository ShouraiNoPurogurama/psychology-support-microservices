using Newtonsoft.Json;

namespace AIModeration.API.Shared.Dtos.Gemini;


public record GeminiContentDto(
    [JsonProperty("role")]
    string Role,
    [JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
