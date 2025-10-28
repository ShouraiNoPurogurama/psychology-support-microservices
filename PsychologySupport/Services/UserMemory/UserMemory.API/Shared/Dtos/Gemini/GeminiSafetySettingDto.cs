using Newtonsoft.Json;

namespace UserMemory.API.Shared.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [JsonProperty("category")]
    string Category,
    [JsonProperty("threshold")]
    string Threshold = "OFF"
);
