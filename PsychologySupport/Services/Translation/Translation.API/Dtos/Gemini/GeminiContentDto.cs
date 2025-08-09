using Newtonsoft.Json;

namespace Translation.API.Dtos.Gemini;


public record GeminiContentDto(
    [JsonProperty("role")]
    string Role,
    [JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
