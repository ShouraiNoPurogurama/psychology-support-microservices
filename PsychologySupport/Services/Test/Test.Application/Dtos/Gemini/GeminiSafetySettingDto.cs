using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [JsonProperty("category")]
    string Category,
    [JsonProperty("threshold")]
    string Threshold = "OFF"
);
