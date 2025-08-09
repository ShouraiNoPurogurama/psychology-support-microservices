using Newtonsoft.Json;

namespace Translation.API.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [JsonProperty("category")]
    string Category,
    [JsonProperty("threshold")]
    string Threshold = "OFF"
);
