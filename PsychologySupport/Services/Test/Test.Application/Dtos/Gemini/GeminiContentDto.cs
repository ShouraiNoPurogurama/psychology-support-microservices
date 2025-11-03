using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;


public record GeminiContentDto(
    [property: JsonProperty("role")]
    string Role,
    [property: JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
