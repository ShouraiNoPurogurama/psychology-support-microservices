using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;


public record GeminiContentDto(
    [JsonProperty("role")]
    string Role,
    [JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
