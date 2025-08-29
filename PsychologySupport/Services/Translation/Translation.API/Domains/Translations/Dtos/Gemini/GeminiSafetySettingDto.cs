using Newtonsoft.Json;

namespace Translation.API.Domains.Translations.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [JsonProperty("category")]
    string Category,
    [JsonProperty("threshold")]
    string Threshold = "OFF"
);
