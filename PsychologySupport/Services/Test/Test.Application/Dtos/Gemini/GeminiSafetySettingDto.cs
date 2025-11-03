using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [property: JsonProperty("category")]
    string Category,
    [property: JsonProperty("threshold")]
    string Threshold = "OFF"
);
